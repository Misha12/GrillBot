using Discord;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Grillbot.Services.Initiable
{
    public class InitService
    {
        private List<IInitiable> Initiables { get; }
        private BotLoggingService BotLoggingService { get; }

        public InitService(IServiceProvider provider, BotLoggingService botLoggingService)
        {
            BotLoggingService = botLoggingService;

            var initiableName = typeof(IInitiable).Name;
            Initiables = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.GetInterface(initiableName) != null)
                .Select(t => (IInitiable)provider.GetService(t))
                .Where(o => o != null)
                .ToList();
        }

        public void Init()
        {
            var stopwatch = new Stopwatch();

            foreach(var service in Initiables)
            {
                stopwatch.Start();
                service.Init();
                stopwatch.Stop();

                BotLoggingService.Write(LogSeverity.Info, $"Initialized service {service.GetType().Name}. Time: {stopwatch.Elapsed}", "INIT_SYNC");
                stopwatch.Reset();
            }
        }

        public async Task InitAsync()
        {
            var stopwatch = new Stopwatch();

            foreach(var service in Initiables)
            {
                stopwatch.Start();
                await service.InitAsync().ConfigureAwait(false);
                stopwatch.Stop();

                BotLoggingService.Write(LogSeverity.Info, $"Initialized service {service.GetType().Name}. Time: {stopwatch.Elapsed}", "INIT_ASYNC");
                stopwatch.Reset();
            }
        }
    }
}
