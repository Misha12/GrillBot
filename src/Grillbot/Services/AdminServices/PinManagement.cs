using Discord;
using Discord.Rest;
using Grillbot.Extensions.Discord;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Services.AdminServices
{
    public class PinManagement
    {
        private ILogger<PinManagement> Logger { get; }

        public PinManagement(ILogger<PinManagement> logger)
        {
            Logger = logger;
        }

        public async Task<int> PinPurgeAsync(ITextChannel channel, int take, int skip)
        {
            var pins = await channel.GetPinnedMessagesAsync();

            if (pins.Count == 0)
                throw new InvalidOperationException($"V kanálu **{channel.Mention}** ještě nebylo nic připnuto.");

            var pinsToRemove = pins
                .OrderByDescending(o => o.CreatedAt)
                .Skip(skip).Take(take)
                .OfType<RestUserMessage>();

            await UnpinMessagesAsync(pinsToRemove);
            return pinsToRemove.Count();
        }

        private async Task UnpinMessagesAsync(IEnumerable<RestUserMessage> messages)
        {
            foreach (var message in messages)
            {
                await UnpinMessageAsync(message);
            }
        }

        private async Task UnpinMessageAsync(RestUserMessage message)
        {
            await message.RemoveAllReactionsAsync();
            await message.UnpinAsync();

            Logger.LogInformation("Unpinned message with ID {0} from {1} posted at {2}",
                message.Id, message.Author.GetFullName(), message.CreatedAt);
        }
    }
}
