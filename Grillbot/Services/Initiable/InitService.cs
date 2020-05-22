using Microsoft.Extensions.DependencyInjection;
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
        private ILogger<InitService> Logger { get; }
        private IServiceProvider Provider { get; }

        public InitService(IServiceProvider provider, ILogger<InitService> logger)
        {
            Logger = logger;
            Provider = provider;
        }

        public void Init()
        {
            foreach (var service in GetInitiables())
            {
                service.Init();
                Logger.LogInformation($"Initialized service {service.GetType().Name} (sync).");
            }
        }

        public async Task InitAsync()
        {
            foreach(var service in GetInitiables())
            {
                await service.InitAsync();
                Logger.LogInformation($"Initialized service {service.GetType().Name} (async).");
            }
        }

        private List<IInitiable> GetInitiables()
        {
            var initiableName = typeof(IInitiable).Name;

            using var scope = Provider.CreateScope();
            return Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.GetInterface(initiableName) != null)
                .Select(t => (IInitiable)scope.ServiceProvider.GetService(t))
                .Where(o => o != null)
                .ToList();
        }
    }
}
