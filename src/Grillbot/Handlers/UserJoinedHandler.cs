using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Grillbot.Services.Initiable;
using Grillbot.Services.InviteTracker;
using Grillbot.Services.Logger;
using Grillbot.Services.Statistics;
using Microsoft.Extensions.DependencyInjection;

namespace Grillbot.Handlers
{
    public class UserJoinedHandler : IDisposable, IInitiable
    {
        private DiscordSocketClient Client { get; }
        private Logger Logger { get; }
        private InternalStatistics InternalStatistics { get; }
        private IServiceProvider Services { get; }

        public UserJoinedHandler(DiscordSocketClient client, Logger logger, InternalStatistics internalStatistics,
            IServiceProvider services)
        {
            Client = client;
            Logger = logger;
            InternalStatistics = internalStatistics;
            Services = services;
        }

        private async Task OnUserJoinedOnServerAsync(SocketGuildUser user)
        {
            InternalStatistics.IncrementEvent("UserJoined");

            await Logger.OnUserJoined(user).ConfigureAwait(false);
            await ProcessInviteTrackerAsync(user);
        }

        private async Task ProcessInviteTrackerAsync(SocketGuildUser user)
        {
            using var scope = Services.CreateScope();
            using var inviteTracker = scope.ServiceProvider.GetService<InviteTrackerService>();

            await inviteTracker.OnUserJoinedAsync(user);
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
