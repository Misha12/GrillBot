using Discord;
using Discord.WebSocket;
using Grillbot.Services.Initiable;
using Grillbot.Services.Statistics;
using Grillbot.Services.UserManagement;
using System;
using System.Threading.Tasks;

namespace Grillbot.Handlers
{
    public class ReactionRemovedHandler : IInitiable, IDisposable
    {
        private DiscordSocketClient Client { get; }
        private EmoteStats EmoteStats { get; }
        private InternalStatistics InternalStatistics { get; }
        private UserService UserService { get; }

        public ReactionRemovedHandler(DiscordSocketClient client, EmoteStats emoteStats, InternalStatistics internalStatistics, UserService userService)
        {
            Client = client;
            EmoteStats = emoteStats;
            InternalStatistics = internalStatistics;
            UserService = userService;
        }

        private async Task OnReactionRemovedAsync(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            InternalStatistics.IncrementEvent("ReactionRemoved");
            await EmoteStats.DecrementFromReaction(reaction).ConfigureAwait(false);
            UserService.DecrementReaction(reaction);
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
