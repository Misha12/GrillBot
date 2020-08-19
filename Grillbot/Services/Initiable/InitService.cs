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
            using var scope = Provider.CreateScope();

            foreach (var service in GetInitiables(scope))
            {
                service.Init();
                Logger.LogInformation($"Initialized service {service.GetType().Name} (sync).");
            }
        }

        public async Task InitAsync()
        {
            using var scope = Provider.CreateScope();

            var tasks = GetInitiables(scope)
                .Select(o => InitServiceAsync(o))
                .ToArray();

            await Task.WhenAll(tasks);
        }

        private async Task InitServiceAsync(Initiable.IInitiable service)
        {
            await service.InitAsync();
            Logger.LogInformation($"Initialized service {service.GetType().Name} (async).");
        }

        private List<IInitiable> GetInitiables(IServiceScope scope)
        {
            var initiableName = typeof(IInitiable).Name;

            return Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.GetInterface(initiableName) != null)
                .Select(t => (IInitiable)scope.ServiceProvider.GetService(t))
                .Where(o => o != null)
                .ToList();
        }
    }
}
