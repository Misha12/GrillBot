using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Grillbot.Services.Initiable
{
    public class InitService
    {
        private List<IInitiable> Initiables { get; }
        private ILogger<InitService> Logger { get; }

        public InitService(IServiceProvider provider, ILogger<InitService> logger)
        {
            Logger = logger;

            var initiableName = typeof(IInitiable).Name;
            Initiables = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.GetInterface(initiableName) != null)
                .Select(t => (IInitiable)provider.GetService(t))
                .Where(o => o != null)
                .ToList();
        }

        public void Init()
        {
            foreach(var service in Initiables)
            {
                service.Init();
                Logger.LogInformation($"Initialized service {service.GetType().Name} (sync).");
            }
        }

        public async Task InitAsync()
        {
            foreach(var service in Initiables)
            {
                await service.InitAsync();
                Logger.LogInformation($"Initialized service {service.GetType().Name} (async).");
            }
        }
    }
}
