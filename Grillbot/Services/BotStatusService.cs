using Grillbot.Database.Repository;
using Grillbot.Helpers;
using Grillbot.Models;
using Grillbot.Models.BotStatus;
using Grillbot.Services.Statistics;
using Microsoft.AspNetCore.Hosting;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Services
{
    public class BotStatusService
    {
        private ChannelStats ChannelStats { get; }
        private IHostingEnvironment HostingEnvironment { get; }
        private Logger.Logger Logger { get; }
        private Statistics.Statistics Statistics { get; }
        private BotDbRepository Repository { get; }

        public BotStatusService(ChannelStats channelStats, IHostingEnvironment hostingEnvironment, Logger.Logger logger,
            Statistics.Statistics statistics, BotDbRepository repository)
        {
            Statistics = statistics;
            HostingEnvironment = hostingEnvironment;
            Logger = logger;
            ChannelStats = channelStats;
            Repository = repository;
        }

        public SimpleBotStatus GetSimpleStatus()
        {
            var process = Process.GetCurrentProcess();

            return new SimpleBotStatus()
            {
                RamUsage = FormatHelper.FormatAsSize(process.WorkingSet64),
                ActiveWebTokensCount = ChannelStats.GetActiveWebTokensCount(),
                InstanceType = GetInstanceType(),
                StartTime = process.StartTime,
                ThreadStatus = GetThreadStatus(process),
                AvgReactTime = Statistics.GetAvgReactTime(),
                ActiveCpuTime = process.TotalProcessorTime
            };
        }

        public List<StatisticsData> GetCallStats() => Statistics.GetOrderedData();

        public Dictionary<string, uint> GetLoggerStats()
        {
            return Logger.Counters.OrderByDescending(o => o.Value).ToDictionary(o => o.Key, o => o.Value);
        }

        private string GetInstanceType()
        {
            if (HostingEnvironment.IsProduction()) return "Release";
            if (HostingEnvironment.IsStaging()) return "Staging";

            return "Development";
        }

        private string GetThreadStatus(Process process)
        {
            int sleepCount = 0;
            var sleepCounter = process.Threads.GetEnumerator();
            while (sleepCounter.MoveNext())
            {
                if ((sleepCounter.Current as ProcessThread)?.ThreadState == ThreadState.Wait)
                    sleepCount++;
            }

            return $"{FormatHelper.FormatWithSpaces(process.Threads.Count)} ({FormatHelper.FormatWithSpaces(sleepCount)} spí)";
        }

        public async Task<Dictionary<string, int>> GetDbReport()
        {
            return await Repository.GetTableRowsCount().ConfigureAwait(false);
        }
    }
}