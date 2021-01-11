using Discord;
using Discord.WebSocket;
using Grillbot.Extensions.Discord;
using Grillbot.Extensions.Infrastructure;
using Grillbot.Services.BackgroundTasks;
using Grillbot.Services.Initiable;
using Grillbot.Services.Statistics;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Grillbot.Handlers
{
    public class ChannelUpdatedHandler : IInitiable, IDisposable, IHandler
    {
        private DiscordSocketClient DiscordClient { get; }
        private IServiceProvider Provider { get; }
        private InternalStatistics InternalStatistics { get; }

        public ChannelUpdatedHandler(DiscordSocketClient client, IServiceProvider serviceProvider, InternalStatistics internalStatistics)
        {
            DiscordClient = client;
            Provider = serviceProvider;
            InternalStatistics = internalStatistics;
        }

        private async Task OnChannelUpdatedAsync(SocketChannel before, SocketChannel after)
        {
            if (after is not SocketGuildChannel afterChannel || before is not SocketGuildChannel beforeChannel) return;
            if (beforeChannel.IsEquals(afterChannel)) return;

            InternalStatistics.IncrementEvent("ChannelUpdated");

            using var scope = Provider.CreateScope();
            scope.ServiceProvider.GetService<BackgroundTaskQueue>().ScheduleDownloadAuditLogIfNotExists(ActionType.ChannelUpdated, afterChannel.Guild, 30);
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
