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
    public class ChannelUpdatedHandler : IInitiable, IDisposable, IHandler
    {
        private DiscordSocketClient DiscordClient { get; }
        private IServiceProvider Provider { get; }

        public ChannelUpdatedHandler(DiscordSocketClient client, IServiceProvider serviceProvider)
        {
            DiscordClient = client;
            Provider = serviceProvider;
        }

        private async Task OnChannelUpdatedAsync(SocketChannel before, SocketChannel after)
        {
            if (after is not SocketGuildChannel ch) return;

            using var scope = Provider.CreateScope();
            scope.ServiceProvider.GetService<BackgroundTaskQueue>().ScheduleDownloadAuditLogIfNotExists(ActionType.ChannelUpdated, ch.Guild, 120);
        }

        public void Init()
        {
            DiscordClient.ChannelUpdated += OnChannelUpdatedAsync;
        }

        public Task InitAsync() => Task.CompletedTask;

        public void Dispose()
        {
            DiscordClient.ChannelUpdated -= OnChannelUpdatedAsync;
        }
    }
}
