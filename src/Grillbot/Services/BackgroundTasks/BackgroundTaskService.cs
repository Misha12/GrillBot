using Grillbot.Models;
using Grillbot.Services.BackgroundTasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Grillbot.Services
{
    public class BackgroundTaskService : BackgroundService
    {
        private BackgroundTaskQueue Queue { get; }
        private ILogger<BackgroundTaskService> Logger { get; }
        private IServiceProvider Provider { get; }

        public BackgroundTaskService(BackgroundTaskQueue queue, ILogger<BackgroundTaskService> logger, IServiceProvider provider)
        {
            Queue = queue;
            Logger = logger;
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

                    if (item?.TaskType == null || item.TaskType.GetInterface(observerType.FullName) == null)
                        continue;

                    await ProcessTaskItemAsync(item);
                }
                finally
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
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
                Logger.LogDebug("Executing background task {0}", task.TaskType.FullName);
                await service.TriggerBackgroundTaskAsync(task);
            }
            catch(Exception ex)
            {
                Logger.LogError(ex, "Executing background task {0} failed.", task.TaskType.FullName);
            }
            finally
            {
                Logger.LogInformation("Background task executing finished ({0})", task.TaskType.FullName);
            }
        }
    }
}