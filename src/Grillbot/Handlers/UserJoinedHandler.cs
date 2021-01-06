using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Grillbot.Extensions.Discord;
using Grillbot.Extensions.Infrastructure;
using Grillbot.Services.Audit;
using Grillbot.Services.BackgroundTasks;
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

            if (!user.IsUser())
                scope.ServiceProvider.GetService<BackgroundTaskQueue>().ScheduleDownloadAuditLog(ActionType.BotAdded, user.Guild);
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
