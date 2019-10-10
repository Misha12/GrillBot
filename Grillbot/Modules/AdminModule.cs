using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Grillbot.Services;
using Grillbot.Services.Preconditions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Modules
{
    [IgnorePM]
    [Name("Administrační funkce")]
    [RequirePermissions("Admin")]
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
                        await pin.UnpinAsync();
                    }

                    await ReplyAsync($"Úpěšně dokončeno. Počet odepnutých zpráv: **{pinsToRemove.Count()}**");
                }
                else
                {
                    throw new ArgumentException($"Odkazovaný kanál **{channel}** nebyl nalezen.");
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
    }
}
