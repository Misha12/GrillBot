using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WatchDog_Bot.Repository;

namespace WatchDog_Bot.Services.Statistics
{
    public class Statistics : IConfigChangeable
    {
        public Dictionary<string, StatisticsData> Data { get; }
        public double AvgReactTime { get; private set; }
        public Dictionary<ulong, long> ChannelCounter { get; private set; }

        private IConfigurationRoot Config { get; set; }

        private Timer DataSyncTimer { get; set; }
        private bool CanUpdateInDB { get; set; }

        public Statistics(IConfigurationRoot configuration)
        {
            Data = new Dictionary<string, StatisticsData>();
            ChannelCounter = new Dictionary<ulong, long>();
            Config = configuration;

            var syncTimerConfig = Convert.ToInt32(configuration["Leaderboards:SyncWithDBSecs"]) * 1000;
            DataSyncTimer = new Timer(SyncTimerCallback, null, syncTimerConfig, syncTimerConfig);
        }

        public void SyncTimerCallback(object _)
        {
            if(CanUpdateInDB)
            {
                using (var repository = new ChannelStatsRepository(Config))
                {
                    repository.SaveStatistics(ChannelCounter).Wait();
                }

                Console.WriteLine($"{DateTime.Now.ToLongTimeString()} BOT\tChannel statistics was synchronized with database. (Rows: {ChannelCounter.Count})");
                CanUpdateInDB = false;
            }
        }

        public async Task Init()
        {
            using(var repository = new ChannelStatsRepository(Config))
            {
                var data = await repository.GetStatistics();
                ChannelCounter = data.ToDictionary(o => o.Item1, o => o.Item2);
            }

            await Console.Out.WriteLineAsync($"{DateTime.Now.ToLongTimeString()} BOT\tChannel statistics loaded from database. (Rows: {ChannelCounter.Count})");
        }

        public void LogCall(string command, long elapsedTime)
        {
            if (!Data.ContainsKey(command))
                Data.Add(command, new StatisticsData(command, elapsedTime));
            else
                Data[command].Increment(elapsedTime);
        }

        public List<StatisticsData> GetOrderedData(bool byTime)
        {
            if (byTime)
                return Data.Values.OrderByDescending(o => o.AverageTime).ToList();

            return Data.Values.OrderByDescending(o => o.CallsCount).ToList();
        }

        public void ComputeAvgReact(long elapsedTime)
        {
            AvgReactTime = (AvgReactTime + elapsedTime) / 2.0D;
        }

        public void IncrementChannelCounter(ulong channelID)
        {
            if (!ChannelCounter.ContainsKey(channelID))
                ChannelCounter.Add(channelID, 1);
            else
                ChannelCounter[channelID]++;

            CanUpdateInDB = true;
        }

        public void DecrementChannelCounter(ulong channelID)
        {
            if (!ChannelCounter.ContainsKey(channelID)) return;
            ChannelCounter[channelID]--;

            CanUpdateInDB = true;
        }

        public void ConfigChanged(IConfigurationRoot newConfig)
        {
            var syncTimerConfig = Convert.ToInt32(newConfig["Leaderboards:SyncWithDBSecs"]) * 1000;
            DataSyncTimer.Change(syncTimerConfig, syncTimerConfig);

            Config = newConfig;
        }
    }
}
