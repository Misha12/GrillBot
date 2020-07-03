using Discord;
using Discord.Commands;
using Grillbot.Attributes;
using Grillbot.Extensions;
using Grillbot.Extensions.Discord;
using Grillbot.Models.Embed;
using Grillbot.Models.Embed.PaginatedEmbed;
using Grillbot.Services;
using Grillbot.Services.Channelboard;
using Grillbot.Services.Permissions.Preconditions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Modules
{
    [RequirePermissions]
    [Group("guild")]
    [Name("Správa discord serveru")]
    [ModuleID("GuildModule")]
    public class GuildModule : BotModuleBase
    {
        private BotStatusService BotStatus { get; }
        private ChannelStats ChannelStats { get; }

        public GuildModule(BotStatusService botStatus, ChannelStats channelStats, PaginationService paginationService)
            : base(paginationService: paginationService)
        {
            BotStatus = botStatus;
            ChannelStats = channelStats;
        }

        [Command("info")]
        [Summary("Informace o serveru")]
        public async Task InfoAsync()
        {
            var guild = Context.Guild;

            var basicEmotesCount = guild.Emotes.Count(o => !o.Animated);
            var animatedCount = guild.Emotes.Count - basicEmotesCount;
            var banCount = (await guild.GetBansAsync()).Count;

            var onlineUsersCount = guild.Users.Count(o => o.Status == UserStatus.Online);
            var offlineUsersCount = guild.Users.Count(o => o.Status == UserStatus.Offline || o.Status == UserStatus.Invisible);
            var idleUsersCount = guild.Users.Count(o => o.Status == UserStatus.Idle || o.Status == UserStatus.AFK);
            var doNotDisturbUsersCount = guild.Users.Count(o => o.Status == UserStatus.DoNotDisturb);

            var color = guild.Roles.FindHighestRoleWithColor()?.Color;
            var embed = new BotEmbed(Context.Message.Author, color, title: guild.Name, thumbnail: guild.IconUrl)
                .AddField("Počet kategorií", (guild.CategoryChannels?.Count ?? 0).FormatWithSpaces(), true)
                .AddField("Počet textových kanálů", guild.ComputeTextChannelsCount().FormatWithSpaces(), true)
                .AddField("Počet hlasových kanálů", guild.ComputeVoiceChannelsCount().FormatWithSpaces(), true)
                .AddField("Počet rolí", guild.Roles.Count.FormatWithSpaces(), true)
                .AddField("Počet běžných emotů", basicEmotesCount.FormatWithSpaces(), true)
                .AddField("Počet animovaných emotů", animatedCount.FormatWithSpaces(), true)
                .AddField("Počet banů", banCount.FormatWithSpaces(), true)
                .AddField("Vytvořen", guild.CreatedAt.DateTime.ToLocaleDatetime(), true)
                .AddField("Vlastník", $"{guild.Owner.GetFullName()} ({guild.OwnerId})", false)
                .AddField("Systémový kanál", $"{guild.SystemChannel?.Name ?? "None"} ({guild.SystemChannel?.Id ?? 0})", false)
                .AddField("Server synchronizován", guild.IsSynced.TranslateToCz(), true)
                .AddField("Počet uživatelů (v paměti)", $"{guild.MemberCount} ({guild.Users.Count})", true)
                .AddField("Úroveň ověření", guild.VerificationLevel.ToString(), true)
                .AddField("Úroveň MFA", guild.MfaLevel.ToString(), true)
                .AddField("Filtr explicitního obsahu", guild.ExplicitContentFilter.ToString(), true)
                .AddField("Výchozí notifikace", guild.DefaultMessageNotifications.ToString(), true)
                .AddField("Extra funkce", guild.Features.Count == 0 ? "-" : string.Join(", ", guild.Features), false)
                .AddField("Tier", guild.PremiumTier.ToString(), true)
                .AddField("Počet boosterů", guild.PremiumSubscriptionCount.FormatWithSpaces(), true)
                .AddField("Stav uživatelů", "_ _", false)
                .AddField("Online", onlineUsersCount.FormatWithSpaces(), true)
                .AddField("Idle", idleUsersCount.FormatWithSpaces(), true)
                .AddField("DoNotDisturb", doNotDisturbUsersCount.FormatWithSpaces(), true)
                .AddField("Offline", offlineUsersCount.FormatWithSpaces(), true);

            await ReplyAsync(embed: embed.Build());
        }

        [Command("sync")]
        [Summary("Synchronizace serveru")]
        public async Task SyncAsync()
        {
            await Context.Guild.SyncGuildAsync();
            await ReplyAsync("Synchronizace úspěšně dokončena.");
        }

        [Command("channels")]
        [Summary("Statistiky kanálů")]
        public async Task ChannelsAsync(string mode = "summary")
        {
            switch (mode)
            {
                case "summary":
                    await ChannelsSummaryAsync();
                    break;
                case "full":
                    await ChannelsListAsync();
                    break;
            }
        }

        private async Task ChannelsSummaryAsync()
        {
            var cache = BotStatus.GetCacheStatus(Context.Guild);
            var channelboard = ChannelStats.GetAllChannels(Context.Guild);

            var internalCacheSummary = cache.Sum(o => o.InternalCacheCount);
            var messageCacheSummary = cache.Sum(o => o.MessageCacheCount);
            var channelboardSummary = channelboard.Sum(o => o.Count);

            var embed = new BotEmbed(Context.User, title: "Souhrn kanálů (počet zpráv)", thumbnail: Context.Guild.IconUrl)
                .AddField("Interní cache", internalCacheSummary.FormatWithSpaces(), true)
                .AddField("Externí cache", messageCacheSummary.FormatWithSpaces(), true)
                .AddField("Celkový počet zpráv", channelboardSummary.FormatWithSpaces(), true);

            await ReplyAsync(embed: embed.Build());
        }

        private async Task ChannelsListAsync()
        {
            var cache = BotStatus.GetCacheStatus(Context.Guild);
            var channelboard = ChannelStats.GetAllChannels(Context.Guild);

            var embed = new PaginatedEmbed()
            {
                Pages = new List<PaginatedEmbedPage>(),
                ResponseFor = Context.User,
                Thumbnail = Context.Guild.IconUrl,
                Title = "Seznam kanálů"
            };

            var chunks = cache.SplitInParts(5);
            foreach (var chunk in chunks)
            {
                var page = new PaginatedEmbedPage(null);

                foreach (var channel in chunk)
                {
                    var channelboardChannel = channelboard.Find(o => o.ChannelIDSnowflake == channel.Channel.Id);
                    var name = channel.Channel.Category == null ? channel.Channel.Name : $"{channel.Channel.Category.Name}/{channel.Channel.Name}";

                    var info = $"Interní cache: **{channel.InternalCacheCount.FormatWithSpaces()}**" +
                        $"\nExterní cache: **{channel.MessageCacheCount.FormatWithSpaces()}**" +
                        $"\nCelkový počet zpráv: **{(channelboardChannel?.Count ?? 0).FormatWithSpaces()}**";

                    page.AddField(name, info);
                }

                if (page.AnyField())
                    embed.Pages.Add(page);
            }

            await SendPaginatedEmbedAsync(embed);
        }

        [Command("channel")]
        [Summary("Informace o kanálu")]
        public async Task ChannelInfoAsync(IChannel channel)
        {
            var cache = BotStatus.GetCacheStatus(Context.Guild, channel);
            var channelboard = ChannelStats.GetChannel(Context.Guild, channel);

            var embed = new BotEmbed(Context.User, title: $"Informace o kanálu #{channel.Name}")
                .AddField("Interní cache", cache.InternalCacheCount.FormatWithSpaces(), true)
                .AddField("Externí cache", cache.MessageCacheCount.FormatWithSpaces(), true)
                .AddField("Celkový počet zpráv", channelboard.Count.FormatWithSpaces(), true)
                .AddField("Poslední zpráva", channelboard.LastMessageAt.ToLocaleDatetime(), true);

            await ReplyAsync(embed: embed.Build());
        }
    }
}
