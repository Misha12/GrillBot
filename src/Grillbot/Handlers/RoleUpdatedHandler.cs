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
    public class RoleUpdatedHandler : IInitiable, IDisposable, IHandler
    {
        private DiscordSocketClient DiscordClient { get; }
        private IServiceProvider Provider { get; }
        private InternalStatistics InternalStatistics { get; }

        public RoleUpdatedHandler(DiscordSocketClient client, IServiceProvider serviceProvider, InternalStatistics internalStatistics)
        {
            DiscordClient = client;
            Provider = serviceProvider;
            InternalStatistics = internalStatistics;
        }

        private async Task OnRoleUpdatedAsync(SocketRole before, SocketRole after)
        {
            InternalStatistics.IncrementEvent("RoleUpdated");

            using var scope = Provider.CreateScope();
            scope.ServiceProvider.GetService<BackgroundTaskQueue>().ScheduleDownloadAuditLogIfNotExists(ActionType.RoleUpdated, after.Guild, 60);
        }

        public void Init()
        {
            DiscordClient.RoleUpdated += OnRoleUpdatedAsync;
        }

        public Task InitAsync() => Task.CompletedTask;

        public void Dispose()
        {
            DiscordClient.RoleUpdated -= OnRoleUpdatedAsync;
        }
    }
}
