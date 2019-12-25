using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Grillbot.Extensions;
using Grillbot.Extensions.Discord;
using Grillbot.Models.Embed;
using Grillbot.Services;
using Grillbot.Services.Logger;
using Grillbot.Services.Preconditions;
using Grillbot.Services.Statistics;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grillbot.Modules
{
    [Name("Administrační funkce")]
    [RequirePermissions("Admin", DisabledForPM = true)]
    public class AdminModule : BotModuleBase
    {
        private TeamSearchService TeamSearchService { get; }
        private Logger Logger { get; }
        private EmoteStats EmoteStats { get; }

        public AdminModule(TeamSearchService teamSearchService, Logger logger, Statistics statistics)
        {
            TeamSearchService = teamSearchService;
            Logger = logger;
            EmoteStats = statistics.EmoteStats;
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
            var searches = await TeamSearchService.Repository.GetAllSearches().Where(o => o.ChannelId == mentionedChannelId).ToListAsync().ConfigureAwait(false);

            if (searches.Count == 0)
            {
                await ReplyAsync($"V kanálu {channel} nikdo nic nehledá.").ConfigureAwait(false);
                return;
            }

            foreach (var search in searches)
            {
                var message = await TeamSearchService.GetMessageAsync(Convert.ToUInt64(search.ChannelId), Convert.ToUInt64(search.MessageId)).ConfigureAwait(false);

                await TeamSearchService.Repository.RemoveSearchAsync(search.Id).ConfigureAwait(false);
                await ReplyAsync($"Hledání s ID **{search.Id}** od **{message.Author.GetShortName()}** smazáno.").ConfigureAwait(false);
            }

            await ReplyAsync($"Čištění kanálu {channel} dokončeno.").ConfigureAwait(false);
        }

        [Command("hledam_mass_remove")]
        [Summary("Hromadné smazání hledání.")]
        public async Task TeamSearchMassRemove(params int[] searchIds)
        {
            foreach (var id in searchIds)
            {
                var search = await TeamSearchService.Repository.FindSearchByID(id).ConfigureAwait(false);

                if (search != null)
                {
                    var message = await TeamSearchService.GetMessageAsync(Convert.ToUInt64(search.ChannelId), Convert.ToUInt64(search.MessageId)).ConfigureAwait(false);

                    if (message == null)
                        await ReplyAsync($"Úklid neznámého hledání s ID **{id}**.").ConfigureAwait(false);
                    else
                        await ReplyAsync($"Úklid hledání s ID **{id}** od **{message.Author.GetFullName()}**.").ConfigureAwait(false);

                    await TeamSearchService.Repository.RemoveSearchAsync(id).ConfigureAwait(false);
                }
            }

            await ReplyAsync($"Úklid hledání s ID **{string.Join(", ", searchIds)}** dokončeno.").ConfigureAwait(false);
        }

        [Command("guild_status")]
        [Summary("Stav serveru")]
        public async Task GuildStatusAsync()
        {
            var guild = Context.Guild;

            var builder = new StringBuilder()
                .Append("Name: **").Append(guild.Name).AppendLine("**")
                .Append("CategoryChannelsCount: **").Append(guild.CategoryChannels?.Count.ToString() ?? "0").AppendLine("**")
                .Append("ChannelsCount: **").Append(guild.Channels.Count.ToString()).AppendLine("**")
                .Append("CreatedAt: **").Append(guild.CreatedAt.ToString()).AppendLine("**")
                .Append("HasAllMembers: **").Append(guild.HasAllMembers.ToString()).AppendLine("**")
                .Append("IsConnected: **").Append(guild.IsConnected.ToString()).AppendLine("**")
                .Append("IsEmbeddable: **").Append(guild.IsEmbeddable.ToString()).AppendLine("**")
                .Append("IsSynced: **").Append(guild.IsSynced.ToString()).AppendLine("**")
                .Append("IconID: **").Append(guild.IconId).AppendLine("**")
                .Append("MemberCount: **").Append(guild.MemberCount.ToString()).AppendLine("**")
                .Append("OwnerID: **").Append(guild.OwnerId.ToString()).AppendLine("**")
                .Append("RolesCount: **").Append(guild.Roles.Count.ToString()).AppendLine("**")
                .Append("SplashID: **").Append(guild.SplashId?.ToString() ?? "null").AppendLine("**")
                .Append("CachedUsersCount: **").Append(guild.Users.Count.ToString()).AppendLine("**")
                .Append("VerificationLevel: **").Append(guild.VerificationLevel.ToString()).AppendLine("**")
                .Append("VoiceRegionID: **").Append(guild.VoiceRegionId ?? "null").AppendLine("**");

            await ReplyAsync(builder.ToString()).ConfigureAwait(false);
        }

        [Command("sync_guild")]
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
                await ReplyAsync($"Synchronizace se nezdařila {ex.Message}").ConfigureAwait(false);
                throw;
            }
        }

        [Command("getTopStackInfo")]
        [Summary("Posledních pet událostí v dané kategorii zapsané do loggeru.")]
        public async Task GetTopStackInfo(string stackKey)
        {
            await DoAsync(async () =>
            {
                var stackInfo = Logger.GetTopStack(stackKey, false);

                if (stackInfo == null)
                    throw new ArgumentException($"Sekce `{stackKey}` neexistuje.");

                var embed = new BotEmbed(Context.Message.Author);

                foreach (var item in stackInfo.Data)
                {
                    embed.AddField(o =>
                    {
                        o.WithName(item.Item1.ToLocaleDatetime());

                        if (!string.IsNullOrEmpty(item.Item2))
                            o.WithValue($"{item.Item2} - {item.Item3}");
                        else
                            o.WithValue(item.Item3);
                    });
                }

                await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        #region EmoteManager

        [Command("EmoteMergeList")]
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
