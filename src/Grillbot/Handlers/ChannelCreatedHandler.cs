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
    public class ChannelCreatedHandler : IInitiable, IDisposable, IHandler
    {
        private DiscordSocketClient DiscordClient { get; }
        private IServiceProvider Provider { get; }

        public ChannelCreatedHandler(DiscordSocketClient client, IServiceProvider serviceProvider)
        {
            DiscordClient = client;
            Provider = serviceProvider;
        }

        private async Task OnChannelCreatedAsync(SocketChannel channel)
        {
            if (channel is not SocketGuildChannel ch) return;

            using var scope = Provider.CreateScope();
            scope.ServiceProvider.GetService<BackgroundTaskQueue>().ScheduleDownloadAuditLog(ActionType.ChannelCreated, ch.Guild);
        }

        public void Init()
        {
            DiscordClient.ChannelCreated += OnChannelCreatedAsync;
        }

        public Task InitAsync() => Task.CompletedTask;

        public void Dispose()
        {
            DiscordClient.ChannelCreated -= OnChannelCreatedAsync;
        }
    }
}
