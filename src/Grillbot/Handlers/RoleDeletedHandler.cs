using Discord;
using Discord.WebSocket;
using Grillbot.Extensions.Infrastructure;
using Grillbot.Services.BackgroundTasks;
using Grillbot.Services.Initiable;
using Grillbot.Services.Statistics;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Grillbot.Handlers
{
    public class RoleDeletedHandler : IInitiable, IDisposable, IHandler
    {
        private DiscordSocketClient DiscordClient { get; }
        private IServiceProvider Provider { get; }
        private InternalStatistics InternalStatistics { get; }

        public RoleDeletedHandler(DiscordSocketClient client, IServiceProvider serviceProvider, InternalStatistics internalStatistics)
        {
            DiscordClient = client;
            Provider = serviceProvider;
            InternalStatistics = internalStatistics;
        }

        private async Task OnRoleDeletedAsync(SocketRole role)
        {
            InternalStatistics.IncrementEvent("RoleDeleted");

            using var scope = Provider.CreateScope();
            scope.ServiceProvider.GetService<BackgroundTaskQueue>().ScheduleDownloadAuditLogIfNotExists(ActionType.RoleDeleted, role.Guild);
        }

        public void Init()
        {
            DiscordClient.RoleDeleted += OnRoleDeletedAsync;
        }

        public Task InitAsync() => Task.CompletedTask;

        public void Dispose()
        {
            DiscordClient.RoleDeleted -= OnRoleDeletedAsync;
        }
    }
}
