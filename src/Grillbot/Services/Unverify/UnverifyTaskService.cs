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
        private const int IntervalCheckSecs = 10;
        private const int MaxPerProcessCount = 5;

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
            // Select all users who will be allowed access and are no longer being processed.
            var keysOfUsers = BotState.UnverifyCache
                .Where(o => (o.Value - DateTime.Now).TotalSeconds <= 0.0F)
                .Take(MaxPerProcessCount)
                .Select(o => o.Key.Split('|'))
                .Where(o => !BotState.CurrentReturningUnverifyFor.Any(x => x.Id.ToString() == o[1]));

            if (!keysOfUsers.Any()) return;

            using var scope = Provider.CreateScope();
            var service = scope.ServiceProvider.GetService<UnverifyService>();

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
            var interval = TimeSpan.FromSeconds(IntervalCheckSecs);
            Timer = new Timer(TimerCallback, null, interval, interval);
        }

        public Task InitAsync() => Task.CompletedTask;
    }
}
