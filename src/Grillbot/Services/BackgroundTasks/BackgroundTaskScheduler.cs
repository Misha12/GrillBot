using Discord;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Grillbot.Services.BackgroundTasks
{
    public class BackgroundTaskScheduler : BackgroundService
    {
        private BackgroundTaskQueue Queue { get; }
        private IServiceProvider Provider { get; }
        private BotLoggingService BotLoggingService { get; }

        private DateTime LastScheduleAt { get; set; }

        public BackgroundTaskScheduler(BackgroundTaskQueue queue, IServiceProvider provider, BotLoggingService botLoggingService)
        {
            Queue = queue;
            Provider = provider;
            BotLoggingService = botLoggingService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var scheduleable = typeof(IBackgroundTaskScheduleable);

            var serviceTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(o => o.GetInterface(scheduleable.FullName) != null)
                .ToList();

            if (serviceTypes.Count == 0)
                return;

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = Provider.CreateScope();

                    foreach (var type in serviceTypes)
                    {
                        var service = scope.ServiceProvider.GetService(type) as IBackgroundTaskScheduleable;

                        if (service.CanScheduleTask(LastScheduleAt))
                        {
                            foreach(var task in service.GetBackgroundTasks())
                            {
                                Queue.Add(task);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    var message = new LogMessage(LogSeverity.Error, nameof(BackgroundTaskScheduler), "Scheduling tasks failed.", ex);
                    await BotLoggingService.OnLogAsync(message);
                }
                finally
                {
                    LastScheduleAt = DateTime.Now;
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
            }
        }
    }
}
