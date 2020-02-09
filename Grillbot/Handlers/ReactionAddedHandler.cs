using Discord;
using Discord.WebSocket;
using Grillbot.Services.Initiable;
using Grillbot.Services.Statistics;
using System;
using System.Threading.Tasks;

namespace Grillbot.Handlers
{
    public class ReactionAddedHandler : IDisposable, IInitiable
    {
        private DiscordSocketClient Client { get; }
        private EmoteStats EmoteStats { get; }
        private CalledEventStats CalledEventStats { get; }

        public ReactionAddedHandler(DiscordSocketClient client, EmoteStats emoteStats, CalledEventStats calledEventStats)
        {
            Client = client;
            EmoteStats = emoteStats;
            CalledEventStats = calledEventStats;
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

        public void Init()
        {
            Client.ReactionAdded += OnReactionAddedAsync;
        }

        public async Task InitAsync() { }
    }
}