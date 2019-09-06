using Discord;
using Discord.WebSocket;
using Grillbot.Services.EmoteStats;
using Grillbot.Services.Statistics;
using System.Threading.Tasks;

namespace Grillbot.Handlers
{
    public class ReactionAddedHandler : IHandle
    {
        private DiscordSocketClient Client { get; }
        private EmoteStats EmoteStats { get; }

        public ReactionAddedHandler(DiscordSocketClient client, Statistics statistics)
        {
            Client = client;
            EmoteStats = statistics.EmoteStats;

            Client.ReactionAdded += OnReactionAddedAsync;
        }

        private async Task OnReactionAddedAsync(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            await EmoteStats.IncrementFromReaction(reaction);
        }

        public void Dispose()
        {
            Client.ReactionAdded -= OnReactionAddedAsync;
        }
    }
}