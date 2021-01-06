using Discord;
using Discord.WebSocket;
using Grillbot.Extensions.Infrastructure;
using Grillbot.Services.BackgroundTasks;
using Grillbot.Services.Initiable;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Grillbot.Handlers
{
    public class RoleCreatedHandler : IInitiable, IDisposable, IHandler
    {
        private DiscordSocketClient DiscordClient { get; }
        private IServiceProvider Provider { get; }

        public RoleCreatedHandler(DiscordSocketClient client, IServiceProvider serviceProvider)
        {
            DiscordClient = client;
            Provider = serviceProvider;
        }

        private async Task OnRoleCreatedAsync(SocketRole role)
        {
            using var scope = Provider.CreateScope();
            scope.ServiceProvider.GetService<BackgroundTaskQueue>().ScheduleDownloadAuditLogIfNotExists(ActionType.RoleCreated, role.Guild);
        }

        public void Init()
        {
            DiscordClient.RoleCreated += OnRoleCreatedAsync;
        }

        public Task InitAsync() => Task.CompletedTask;

        public void Dispose()
        {
            DiscordClient.RoleCreated -= OnRoleCreatedAsync;
        }
    }
}
