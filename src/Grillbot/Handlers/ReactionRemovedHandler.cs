using Discord;
using Discord.WebSocket;
using Grillbot.Services.Initiable;
using Grillbot.Services.Statistics;
using Grillbot.Services.UserManagement;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Grillbot.Handlers
{
    public class ReactionRemovedHandler : IInitiable, IDisposable
    {
        private DiscordSocketClient Client { get; }
        private InternalStatistics InternalStatistics { get; }
        private UserService UserService { get; }
        private IServiceProvider Provider { get; }

        public ReactionRemovedHandler(DiscordSocketClient client, InternalStatistics internalStatistics, UserService userService, IServiceProvider provider)
        {
            Client = client;
            InternalStatistics = internalStatistics;
            UserService = userService;
            Provider = provider;
        }

        private async Task OnReactionRemovedAsync(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            using var scope = Provider.CreateScope();

            InternalStatistics.IncrementEvent("ReactionRemoved");
            await scope.ServiceProvider.GetService<EmoteStats>().DecrementFromReactionAsync(reaction);
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
