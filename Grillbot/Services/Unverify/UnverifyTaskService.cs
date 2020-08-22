using Grillbot.Services.Initiable;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grillbot.Services.Unverify
{
    public class UnverifyTaskService : IInitiable, IDisposable
    {
        private IServiceProvider Provider { get; }
        private BotState BotState { get; }

        private Timer Timer { get; set; }

        public UnverifyTaskService(IServiceProvider provider, BotState botState)
        {
            Provider = provider;
            BotState = botState;
        }

        private void TimerCallback(object _)
        {
            using var scope = Provider.CreateScope();
            using var service = scope.ServiceProvider.GetService<UnverifyService>();

            var keysOfUsers = BotState.UnverifyCache
                .Where(o => (o.Value - DateTime.Now).TotalSeconds <= 0.0F)
                .Select(o => o.Key.Split('|'));

            var unverifiesToReturn = keysOfUsers
                .Select(o => service.AutoUnverifyRemoveAsync(o[0], o[1]))
                .ToArray();

            Task.WaitAll(unverifiesToReturn);

            foreach (var key in keysOfUsers)
            {
                BotState.UnverifyCache.Remove(string.Join("|", key));
            }
        }

        public void Dispose()
        {
            Timer?.Dispose();
        }

        public void Init()
        {
            var interval = TimeSpan.FromSeconds(10);
            Timer = new Timer(TimerCallback, null, interval, interval);
        }

        public Task InitAsync() => Task.CompletedTask;
    }
}
