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
    public class GuildUpdatedHandler : IInitiable, IDisposable, IHandler
    {
        private DiscordSocketClient DiscordClient { get; }
        private IServiceProvider Provider { get; }

        public GuildUpdatedHandler(DiscordSocketClient client, IServiceProvider serviceProvider)
        {
            DiscordClient = client;
            Provider = serviceProvider;
        }

        private async Task OnGuildUpdatedAsync(SocketGuild before, SocketGuild after)
        {
            using var scope = Provider.CreateScope();
            scope.ServiceProvider.GetService<BackgroundTaskQueue>().ScheduleDownloadAuditLogIfNotExists(ActionType.GuildUpdated, after, 60); // Guild updates wait 1 minute to process all changes.
        }

        public void Init()
        {
            DiscordClient.GuildUpdated += OnGuildUpdatedAsync;
        }

        public Task InitAsync() => Task.CompletedTask;

        public void Dispose()
        {
            DiscordClient.GuildUpdated -= OnGuildUpdatedAsync;
        }
    }
}
