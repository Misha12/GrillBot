using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Grillbot.Services.Audit;
using Grillbot.Services.Initiable;
using Grillbot.Services.InviteTracker;
using Grillbot.Services.Statistics;
using Microsoft.Extensions.DependencyInjection;

namespace Grillbot.Handlers
{
    public class UserJoinedHandler : IDisposable, IInitiable
    {
        private DiscordSocketClient Client { get; }
        private InternalStatistics InternalStatistics { get; }
        private IServiceProvider Services { get; }

        public UserJoinedHandler(DiscordSocketClient client, InternalStatistics internalStatistics,
            IServiceProvider services)
        {
            Client = client;
            InternalStatistics = internalStatistics;
            Services = services;
        }

        private async Task OnUserJoinedOnServerAsync(SocketGuildUser user)
        {
            InternalStatistics.IncrementEvent("UserJoined");

            using var scope = Services.CreateScope();

            await scope.ServiceProvider.GetService<AuditService>().LogUserJoinAsync(user);
            await scope.ServiceProvider.GetService<InviteTrackerService>().OnUserJoinedAsync(user);
        }

        #region IDisposable Support

        public void Dispose()
        {
            Client.UserJoined -= OnUserJoinedOnServerAsync;
        }

        #endregion

        public void Init()
        {
            Client.UserJoined += OnUserJoinedOnServerAsync;
        }

        public async Task InitAsync() { }
    }
}
