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
    public class UnbanHandler : IInitiable, IDisposable
    {
        private DiscordSocketClient DiscordClient { get; }
        private InternalStatistics InternalStatistics { get; }
        private IServiceProvider ServiceProvider { get; }

        public UnbanHandler(DiscordSocketClient discordClient, InternalStatistics internalStatistics, IServiceProvider serviceProvider)
        {
            DiscordClient = discordClient;
            ServiceProvider = serviceProvider;
            InternalStatistics = internalStatistics;
        }

        public void Init()
        {
            DiscordClient.UserUnbanned += OnUserUnbannedAsync;
        }

        public Task InitAsync() => Task.CompletedTask;

        private async Task OnUserUnbannedAsync(SocketUser user, SocketGuild guild)
        {
            InternalStatistics.IncrementEvent("UserUnbanned");

            using var scope = ServiceProvider.CreateScope();

            scope.ServiceProvider.GetService<BackgroundTaskQueue>().ScheduleDownloadAuditLog(ActionType.Unban, guild);
        }

        public void Dispose()
        {
            DiscordClient.UserUnbanned -= OnUserUnbannedAsync;
        }
    }
}
