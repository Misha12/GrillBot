using Discord;
using Grillbot.Models;
using Grillbot.Services.BackgroundTasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Grillbot.Services
{
    public class BackgroundTaskService : BackgroundService
    {
        private BackgroundTaskQueue Queue { get; }
        private BotLoggingService BotLoggingService { get; }
        private IServiceProvider Provider { get; }

        public BackgroundTaskService(BackgroundTaskQueue queue, IServiceProvider provider, BotLoggingService botLoggingService)
        {
            Queue = queue;
            BotLoggingService = botLoggingService;
            Provider = provider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var observerType = typeof(IBackgroundTaskObserver);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var item = Queue.PopAvailable();

                    while(item != null)
                    {
                        if (item?.TaskType == null || item.TaskType.GetInterface(observerType.FullName) == null)
                            continue;

                        await ProcessTaskItemAsync(item);
                        item = Queue.PopAvailable();
                    }
                }
                finally
                {
                    await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                }
            }
        }

        private async Task ProcessTaskItemAsync(BackgroundTask task)
        {
            using var scope = Provider.CreateScope();
            var service = (IBackgroundTaskObserver)scope.ServiceProvider.GetService(task.TaskType);

            if (service == null)
                return;

            try
            {
                var logMessage = new LogMessage(LogSeverity.Debug, nameof(BackgroundTaskService), $"Executing background task {task.TaskType.FullName}");
                await BotLoggingService.OnLogAsync(logMessage);

                await service.TriggerBackgroundTaskAsync(task);
            }
            catch(Exception ex)
            {
                var logMessage = new LogMessage(LogSeverity.Error, nameof(BackgroundTaskService), $"Executing background task {task.TaskType.FullName} failed.", ex);
                await BotLoggingService.OnLogAsync(logMessage);
            }
            finally
            {
                var logMessage = new LogMessage(LogSeverity.Info, nameof(BackgroundTaskService), $"Background task executing finished ({task.TaskType.FullName})");
                await BotLoggingService.OnLogAsync(logMessage);
            }
        }
    }
}