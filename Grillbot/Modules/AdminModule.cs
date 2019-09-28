using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Grillbot.Services.Preconditions;
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
    }
}
