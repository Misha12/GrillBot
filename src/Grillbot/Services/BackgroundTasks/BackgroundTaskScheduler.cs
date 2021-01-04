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

        private Dictionary<Type, DateTime> LastSchedulesAt { get; }

        public BackgroundTaskScheduler(BackgroundTaskQueue queue, IServiceProvider provider, BotLoggingService botLoggingService)
        {
            Queue = queue;
            Provider = provider;
            BotLoggingService = botLoggingService;

            LastSchedulesAt = new Dictionary<Type, DateTime>();
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
                        if (scope.ServiceProvider.GetService(type) is not IBackgroundTaskScheduleable service)
                            continue;

                        if (service.CanScheduleTask(GetLastSchedule(type)))
                        {
                            foreach(var task in service.GetBackgroundTasks())
                            {
                                Queue.Add(task);
                            }

                            LastSchedulesAt[type] = DateTime.Now;
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
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
            }
        }

        private DateTime GetLastSchedule(Type type)
        {
            if (!LastSchedulesAt.ContainsKey(type))
                return DateTime.MinValue;

            return LastSchedulesAt[type];
        }
    }
}
