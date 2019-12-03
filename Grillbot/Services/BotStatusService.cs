using Grillbot.Helpers;
using Grillbot.Models;
using Grillbot.Models.BotStatus;
using Grillbot.Modules;
using Grillbot.Repository;
using Grillbot.Repository.Entity;
using Grillbot.Services.Config.Models;
using Grillbot.Services.Statistics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Services
{
    public class BotStatusService
    {
        private Statistics.Statistics Statistics { get; }
        private IHostingEnvironment HostingEnvironment { get; }
        private Logger.Logger Logger { get; }
        private AutoReplyService AutoReplyService { get; }
        private Configuration Config { get; }
        private CalledEventStats CalledEventStats { get; }

        public BotStatusService(Statistics.Statistics statistics, IHostingEnvironment hostingEnvironment, Logger.Logger logger, 
            AutoReplyService autoReplyService, IOptions<Configuration> config, CalledEventStats calledEventStats)
        {
            Statistics = statistics;
            HostingEnvironment = hostingEnvironment;
            Logger = logger;
            AutoReplyService = autoReplyService;
            Config = config.Value;
            CalledEventStats = calledEventStats;
        }

        public SimpleBotStatus GetSimpleStatus()
        {
            var process = Process.GetCurrentProcess();

            return new SimpleBotStatus()
            {
                RamUsage = FormatHelper.FormatAsSize(process.WorkingSet64),
                ActiveWebTokensCount = Statistics.ChannelStats.GetActiveWebTokensCount(),
                InstanceType = GetInstanceType(),
                StartTime = process.StartTime,
                ThreadStatus = GetThreadStatus(process),
                AvgReactTime = Statistics.GetAvgReactTime()
            };
        }

        public List<StatisticsData> GetCallStats(bool byTime = false) => Statistics.GetOrderedData(byTime);

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
                if ((sleepCounter.Current as ProcessThread)?.ThreadState == ThreadState.Wait)
                    sleepCount++;

            return $"{FormatHelper.FormatWithSpaces(process.Threads.Count)} ({FormatHelper.FormatWithSpaces(sleepCount)} spí)";
        }

        public List<AutoReplyItem> GetAutoReplyItems() => AutoReplyService.GetItems();

        public Dictionary<string, string> GetCalledEventStats() => CalledEventStats.GetValues();

        public async Task<Dictionary<string, int>> GetDbReport()
        {
            using(var repository = new BotDbRepository(Config))
            {
                return await repository.GetTableRowsCount().ConfigureAwait(false);
            }
        }
    }
}
