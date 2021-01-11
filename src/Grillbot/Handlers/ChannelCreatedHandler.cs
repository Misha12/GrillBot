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
    public class ChannelCreatedHandler : IInitiable, IDisposable, IHandler
    {
        private DiscordSocketClient DiscordClient { get; }
        private IServiceProvider Provider { get; }
        private InternalStatistics InternalStatistics { get; }

        public ChannelCreatedHandler(DiscordSocketClient client, IServiceProvider serviceProvider, InternalStatistics internalStatistics)
        {
            DiscordClient = client;
            Provider = serviceProvider;
            InternalStatistics = internalStatistics;
        }

        private async Task OnChannelCreatedAsync(SocketChannel channel)
        {
            if (channel is not SocketGuildChannel ch) return;

            InternalStatistics.IncrementEvent("ChannelCreated");

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
