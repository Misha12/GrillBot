using Discord;
using Discord.WebSocket;
using Grillbot.Services;
using Grillbot.Services.Statistics;
using System.Threading.Tasks;

namespace Grillbot.Handlers
{
    public class ReactionAddedHandler : IHandle
    {
        private DiscordSocketClient Client { get; }
        private EmoteStats EmoteStats { get; }
        private CalledEventStats CalledEventStats { get; }

        public ReactionAddedHandler(DiscordSocketClient client, Statistics statistics, CalledEventStats calledEventStats)
        {
            Client = client;
            EmoteStats = statistics.EmoteStats;
            CalledEventStats = calledEventStats;

            Client.ReactionAdded += OnReactionAddedAsync;
        }

        private async Task OnReactionAddedAsync(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            CalledEventStats.Increment("ReactionAdded");
            await EmoteStats.IncrementFromReaction(reaction).ConfigureAwait(false);
        }

        public void Dispose()
        {
            Client.ReactionAdded -= OnReactionAddedAsync;
        }
    }
}