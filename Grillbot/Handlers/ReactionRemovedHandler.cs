using Discord;
using Discord.WebSocket;
using Grillbot.Services;
using Grillbot.Services.Statistics;
using System.Threading.Tasks;

namespace Grillbot.Handlers
{
    public class ReactionRemovedHandler : IHandle
    {
        private DiscordSocketClient Client { get; }
        private EmoteStats EmoteStats { get; }

        public ReactionRemovedHandler(DiscordSocketClient client, Statistics statistics)
        {
            Client = client;
            EmoteStats = statistics.EmoteStats;

            Client.ReactionRemoved += OnReactionRemovedAsync;
        }

        private async Task OnReactionRemovedAsync(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            await EmoteStats.DecrementFromReaction(reaction);
        }

        public void Dispose()
        {
            Client.ReactionRemoved -= OnReactionRemovedAsync;
        }
    }
}
