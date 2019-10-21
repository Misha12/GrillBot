using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Grillbot.Services;
using Grillbot.Services.Preconditions;
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

        public AdminModule(TeamSearchService teamSearchService)
        {
            TeamSearchService = teamSearchService;
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

                if(mentionedChannel != null)
                {
                    var pins = await mentionedChannel.GetPinnedMessagesAsync();

                    if (pins.Count == 0)
                        throw new ArgumentException($"V kanálu **{mentionedChannel.Mention}** ještě nebylo nic připnuto.");

                    var pinsToRemove = pins
                        .OrderByDescending(o => o.CreatedAt)
                        .Skip(skipCount).Take(takeCount)
                        .OfType<RestUserMessage>();

                    foreach(var pin in pinsToRemove)
                    {
                        await pin.RemoveAllReactionsAsync();
                        await pin.UnpinAsync();
                    }

                    await ReplyAsync($"Úpěšně dokončeno. Počet odepnutých zpráv: **{pinsToRemove.Count()}**");
                }
                else
                {
                    throw new ArgumentException($"Odkazovaný textový kanál **{channel}** nebyl nalezen.");
                }
            });
        }

        [Command("hledam_clean_channel")]
        [Summary("Smazání všech hledání v zadaném kanálu.")]
        public async Task TeamSearchCleanChannel(string channel)
        {
            var mentionedChannelId = Context.Message.MentionedChannels.First().Id.ToString();
            var searches = await TeamSearchService.Repository.GetAllSearches().Where(o => o.ChannelId == mentionedChannelId).ToListAsync();

            if(searches.Count == 0)
            {
                await ReplyAsync($"V kanálu {channel} nikdo nic nehledá.");
                return;
            }

            foreach(var search in searches)
            {
                var message = await TeamSearchService.GetMessageAsync(Convert.ToUInt64(search.ChannelId), Convert.ToUInt64(search.MessageId));

                await TeamSearchService.Repository.RemoveSearchAsync(search.Id);
                await ReplyAsync($"Hledání s ID **{search.Id}** od **{GetUsersFullName(message.Author)}** smazáno.");
            }

            await ReplyAsync($"Čištění kanálu {channel} dokončeno.");
        }

        [Command("hledam_mass_remove")]
        [Summary("Hromadné smazání hledání.")]
        public async Task TeamSearchMassRemove(params int[] searchIds)
        {
            foreach(var id in searchIds)
            {
                var search = await TeamSearchService.Repository.FindSearchByID(id);

                if(search != null)
                {
                    var message = await TeamSearchService.GetMessageAsync(Convert.ToUInt64(search.ChannelId), Convert.ToUInt64(search.MessageId));

                    if(message == null)
                        await ReplyAsync($"Úklid neznámého hledání s ID **{id}**.");
                    else
                        await ReplyAsync($"Úklid hledání s ID **{id}** od **{GetUsersFullName(message.Author)}**.");

                    await TeamSearchService.Repository.RemoveSearchAsync(id);
                }
            }

            await ReplyAsync($"Úklid hledání s ID **{string.Join(", ", searchIds)}** dokončeno.");
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

            await ReplyAsync(builder.ToString());
        }

        [Command("sync_guild")]
        [Summary("Synchronizace serveru s botem.")]
        public async Task SyncGuild()
        {
            var guild = Context.Guild;

            try
            {
                await guild.DownloadUsersAsync();

                if(guild.SyncPromise != null)
                    await guild.SyncPromise;

                if(guild.DownloaderPromise != null)
                    await guild.DownloaderPromise;

                await ReplyAsync("Synchronizace dokončena");
            }
            catch (Exception ex)
            {
                await ReplyAsync($"Synchronizace se nezdařila {ex.Message}");
                throw;
            }
        }
    }
}
