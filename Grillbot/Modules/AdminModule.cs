using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Grillbot.Extensions;
using Grillbot.Extensions.Discord;
using Grillbot.Models.Embed;
using Grillbot.Services;
using Grillbot.Services.Preconditions;
using Grillbot.Services.Statistics;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Modules
{
    [Name("Administrační funkce")]
    [RequirePermissions("Admin", DisabledForPM = true)]
    public class AdminModule : BotModuleBase
    {
        private TeamSearchService TeamSearchService { get; }
        private EmoteStats EmoteStats { get; }

        public AdminModule(TeamSearchService teamSearchService, EmoteStats emoteStats)
        {
            TeamSearchService = teamSearchService;
            EmoteStats = emoteStats;
        }

        [Command("pinpurge")]
        [Summary("Hromadné odpinování zpráv.")]
        [Remarks("Poslední parametr skipCount je volitelný. Výchozí hodnota je 0.")]
        public async Task PinPurge(string channel, int takeCount, int skipCount = 0)
        {
            await DoAsync(async () =>
            {
                var mentionedChannel = Context.Message.MentionedChannels
                    .OfType<SocketTextChannel>()
                    .FirstOrDefault(o => $"<#{o.Id}>" == channel);

                if (mentionedChannel != null)
                {
                    var pins = await mentionedChannel.GetPinnedMessagesAsync().ConfigureAwait(false);

                    if (pins.Count == 0)
                        throw new ArgumentException($"V kanálu **{mentionedChannel.Mention}** ještě nebylo nic připnuto.");

                    var pinsToRemove = pins
                        .OrderByDescending(o => o.CreatedAt)
                        .Skip(skipCount).Take(takeCount)
                        .OfType<RestUserMessage>();

                    foreach (var pin in pinsToRemove)
                    {
                        await pin.RemoveAllReactionsAsync().ConfigureAwait(false);
                        await pin.UnpinAsync().ConfigureAwait(false);
                    }

                    await ReplyAsync($"Úpěšně dokončeno. Počet odepnutých zpráv: **{pinsToRemove.Count()}**").ConfigureAwait(false);
                }
                else
                {
                    throw new ArgumentException($"Odkazovaný textový kanál **{channel}** nebyl nalezen.");
                }
            }).ConfigureAwait(false);
        }

        [Command("hledam_clean_channel")]
        [Summary("Smazání všech hledání v zadaném kanálu.")]
        public async Task TeamSearchCleanChannel(string channel)
        {
            var mentionedChannelId = Context.Message.MentionedChannels.First().Id.ToString();
            var searches = await TeamSearchService.Repository.TeamSearch.GetAllSearches().Where(o => o.ChannelId == mentionedChannelId).ToListAsync().ConfigureAwait(false);

            if (searches.Count == 0)
            {
                await ReplyAsync($"V kanálu {channel.PreventMassTags()} nikdo nic nehledá.").ConfigureAwait(false);
                return;
            }

            foreach (var search in searches)
            {
                var message = await TeamSearchService.GetMessageAsync(Convert.ToUInt64(search.ChannelId), Convert.ToUInt64(search.MessageId)).ConfigureAwait(false);

                await TeamSearchService.Repository.TeamSearch.RemoveSearchAsync(search.Id).ConfigureAwait(false);
                await ReplyAsync($"Hledání s ID **{search.Id}** od **{message.Author.GetShortName()}** smazáno.").ConfigureAwait(false);
            }

            await ReplyAsync($"Čištění kanálu {channel.PreventMassTags()} dokončeno.").ConfigureAwait(false);
        }

        [Command("hledam_mass_remove")]
        [Summary("Hromadné smazání hledání.")]
        public async Task TeamSearchMassRemove(params int[] searchIds)
        {
            foreach (var id in searchIds)
            {
                var search = await TeamSearchService.Repository.TeamSearch.FindSearchByID(id).ConfigureAwait(false);

                if (search != null)
                {
                    var message = await TeamSearchService.GetMessageAsync(Convert.ToUInt64(search.ChannelId), Convert.ToUInt64(search.MessageId)).ConfigureAwait(false);

                    if (message == null)
                        await ReplyAsync($"Úklid neznámého hledání s ID **{id}**.").ConfigureAwait(false);
                    else
                        await ReplyAsync($"Úklid hledání s ID **{id}** od **{message.Author.GetFullName()}**.").ConfigureAwait(false);

                    await TeamSearchService.Repository.TeamSearch.RemoveSearchAsync(id).ConfigureAwait(false);
                }
            }

            await ReplyAsync($"Úklid hledání s ID **{string.Join(", ", searchIds)}** dokončeno.").ConfigureAwait(false);
        }

        [Command("guildStatus")]
        [Summary("Stav serveru")]
        public async Task GuildStatusAsync()
        {
            var guild = Context.Guild;

            var embed = new BotEmbed(Context.Message.Author, title: guild.Name)
                .WithThumbnail(guild.IconUrl)
                .WithFields(
                    new EmbedFieldBuilder().WithName("CategoryChannelsCount").WithValue($"**{guild.CategoryChannels?.Count ?? 0}**").WithIsInline(true),
                    new EmbedFieldBuilder().WithName("ChannelsCount").WithValue($"**{guild.Channels.Count}**").WithIsInline(true),
                    new EmbedFieldBuilder().WithName("CreatedAt").WithValue($"**{guild.CreatedAt.DateTime.ToLocaleDatetime()}**").WithIsInline(true),
                    new EmbedFieldBuilder().WithName("HasAllMembers").WithValue($"**{guild.HasAllMembers}**").WithIsInline(true),
                    new EmbedFieldBuilder().WithName("IsEmbeddable").WithValue($"**{guild.IsEmbeddable}**").WithIsInline(true),
                    new EmbedFieldBuilder().WithName("IsSynced").WithValue($"**{guild.IsSynced}**").WithIsInline(true),
                    new EmbedFieldBuilder().WithName("MemberCount").WithValue($"**{guild.MemberCount}**").WithIsInline(true),
                    new EmbedFieldBuilder().WithName("CachedUsersCount").WithValue($"**{guild.Users.Count}**").WithIsInline(true),
                    new EmbedFieldBuilder().WithName("RolesCount").WithValue($"**{guild.Roles.Count}**").WithIsInline(true),
                    new EmbedFieldBuilder().WithName("OwnerID").WithValue($"**{guild.OwnerId}**").WithIsInline(true),
                    new EmbedFieldBuilder().WithName("SplashID").WithValue($"**{guild.SplashId?.ToString() ?? "null"}**").WithIsInline(true),
                    new EmbedFieldBuilder().WithName("IconID").WithValue($"**{guild.IconId}**").WithIsInline(true),
                    new EmbedFieldBuilder().WithName("VerificationLevel").WithValue($"**{guild.VerificationLevel}**").WithIsInline(true),
                    new EmbedFieldBuilder().WithName("VoiceRegionID").WithValue($"**{guild.VoiceRegionId ?? "null"}**").WithIsInline(true),
                    new EmbedFieldBuilder().WithName("MfaLevel").WithValue($"**{guild.MfaLevel}**").WithIsInline(true),
                    new EmbedFieldBuilder().WithName("ExplicitContentFilter").WithValue($"**{guild.ExplicitContentFilter}**").WithIsInline(true),
                    new EmbedFieldBuilder().WithName("SystemChannel").WithValue($"**{guild.SystemChannel?.Name ?? "None"}**").WithIsInline(true),
                    new EmbedFieldBuilder().WithName("DefaultMessageNotifications").WithValue($"**{guild.DefaultMessageNotifications}**").WithIsInline(true)
                );

            await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
        }

        [Command("syncGuild")]
        [Summary("Synchronizace serveru s botem.")]
        public async Task SyncGuild()
        {
            var guild = Context.Guild;

            try
            {
                await guild.DownloadUsersAsync().ConfigureAwait(false);

                if (guild.SyncPromise != null)
                    await guild.SyncPromise.ConfigureAwait(false);

                if (guild.DownloaderPromise != null)
                    await guild.DownloaderPromise.ConfigureAwait(false);

                await ReplyAsync("Synchronizace dokončena").ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await ReplyAsync($"Synchronizace se nezdařila {ex.Message.PreventMassTags()}").ConfigureAwait(false);
                throw;
            }
        }

        #region EmoteManager

        [Command("emoteMergeList")]
        [Summary("Seznam potenciálních emotů, které by měli být sloučeny.")]
        public async Task GetMergeList()
        {
            await DoAsync(async () =>
            {
                var list = EmoteStats.GetMergeList(Context.Guild);

                if (list.Count == 0)
                    throw new ArgumentException("Aktuálně není nic ke sloučení.");

                var embed = new BotEmbed(Context.Message.Author, title: "Seznam potenciálních sloučení emotů");

                embed.WithFields(list.Select(o => new EmbedFieldBuilder()
                {
                    Name = $"Target: \\{o.MergeTo}",
                    Value = $"Sources: {Environment.NewLine}{string.Join(Environment.NewLine, o.Emotes.Select(x => $"[\\{x.Key}, {x.Value}]"))}"
                }));

                await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        [Command("ProcessEmoteMerge")]
        [Summary("Provede sloučení stejných emotů ve statistikách.")]
        public async Task ProcessEmoteMerge()
        {
            await DoAsync(async () =>
            {
                await EmoteStats.MergeEmotesAsync(Context.Guild).ConfigureAwait(false);
                await ReplyAsync("Sloučení dokončeno").ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        #endregion
    }
}
