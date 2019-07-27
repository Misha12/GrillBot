using GrilBot.Repository;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GrilBot.Services.Statistics
{
    public class ChannelStats : IConfigChangeable, IDisposable
    {
        public Dictionary<ulong, long> Counter { get; private set; }
        private IConfigurationRoot Config { get; set; }
        private Timer DbSyncTimer { get; set; }
        private List<ulong> Changes { get; set; }
        private SemaphoreSlim Semaphore { get; set; }

        public ChannelStats(IConfigurationRoot config)
        {
            Counter = new Dictionary<ulong, long>();
            Changes = new List<ulong>();

            Config = config;
            Semaphore = new SemaphoreSlim(1, 1);
        }

        public async Task Init()
        {
            using (var repository = new ChannelStatsRepository(Config))
            {
                var data = await repository.GetStatistics();
                Counter = data.ToDictionary(o => o.Item1, o => o.Item2);
            }

            Reload(Config);
            await Console.Out.WriteLineAsync($"{DateTime.Now.ToLongTimeString()} BOT\tChannel statistics loaded from database. (Rows: {Counter.Count})");
        }

        private void Reload(IConfigurationRoot config)
        {
            var syncTimerConfig = Convert.ToInt32(config["Leaderboards:SyncWithDBSecs"]) * 1000;
            DbSyncTimer = new Timer(SyncTimerCallback, null, syncTimerConfig, syncTimerConfig);
        }

        public void ConfigChanged(IConfigurationRoot newConfig)
        {
            Reload(newConfig);
        }

        private void SyncTimerCallback(object _)
        {
            Semaphore.Wait();

            try
            {
                if (Changes.Count == 0) return;
                var forUpdate = Counter.Where(o => Changes.Contains(o.Key)).ToDictionary(o => o.Key, o => o.Value);

                using (var repository = new ChannelStatsRepository(Config))
                {
                    repository.UpdateStatistics(forUpdate).Wait();
                }

                Changes.Clear();
                Console.WriteLine($"{DateTime.Now.ToLongTimeString()} BOT\tChannel statistics was synchronized with database. (Updated {forUpdate.Count} records)");
            }
            finally
            {
                Semaphore.Release();
            }
        }

        public async Task IncrementCounter(ulong channelID)
        {
            await Semaphore.WaitAsync();

            try
            {
                if (!Counter.ContainsKey(channelID))
                    Counter.Add(channelID, 1);
                else
                    Counter[channelID]++;

                if (!Changes.Contains(channelID))
                    Changes.Add(channelID);
            }
            finally
            {
                Semaphore.Release();
            }
        }

        public async Task DecrementCounter(ulong channelID)
        {
            await Semaphore.WaitAsync();

            try
            {
                if (!Counter.ContainsKey(channelID)) return;

                Counter[channelID]--;

                if (Counter[channelID] < 0)
                    Counter[channelID] = 0;

                if (!Changes.Contains(channelID))
                    Changes.Add(channelID);
            }
            finally
            {
                Semaphore.Release();
            }
        }

        public Tuple<int, long> GetValue(ulong channelID)
        {
            if (!Counter.ContainsKey(channelID))
                return new Tuple<int, long>(0, 0);

            var orderedData = Counter.OrderByDescending(o => o.Value).ToDictionary(o => o.Key, o => o.Value);
            var keyPosition = Counter.Keys.ToList().FindIndex(o => o == channelID) + 1;
            return new Tuple<int, long>(keyPosition, orderedData[channelID]);
        }

        #region IDisposable Support

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                DbSyncTimer.Dispose();
                Semaphore.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
