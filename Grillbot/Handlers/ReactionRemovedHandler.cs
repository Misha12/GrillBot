using Discord;
using Discord.WebSocket;
using Grillbot.Services.Initiable;
using Grillbot.Services.Statistics;
using System;
using System.Threading.Tasks;

namespace Grillbot.Handlers
{
    public class ReactionRemovedHandler : IInitiable, IDisposable
    {
        private DiscordSocketClient Client { get; }
        private EmoteStats EmoteStats { get; }
        private CalledEventStats CalledEventStats { get; }

        public ReactionRemovedHandler(DiscordSocketClient client, EmoteStats emoteStats, CalledEventStats calledEventStats)
        {
            Client = client;
            EmoteStats = emoteStats;
            CalledEventStats = calledEventStats;
        }

        private async Task OnReactionRemovedAsync(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            CalledEventStats.Increment("ReactionRemoved");
            await EmoteStats.DecrementFromReaction(reaction).ConfigureAwait(false);
        }

        public void Dispose()
        {
            Client.ReactionRemoved -= OnReactionRemovedAsync;
        }

        public void Init()
        {
            Client.ReactionRemoved += OnReactionRemovedAsync;
        }

        public async Task InitAsync() { }
    }
}
