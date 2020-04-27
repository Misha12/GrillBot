using Grillbot.Database.Repository;
using Grillbot.Helpers;
using Grillbot.Models.BotStatus;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Services
{
    public class BotStatusService : IDisposable
    {
        private IWebHostEnvironment HostingEnvironment { get; }
        private Logger.Logger Logger { get; }
        private BotDbRepository Repository { get; }

        public BotStatusService(IWebHostEnvironment hostingEnvironment, Logger.Logger logger, BotDbRepository repository)
        {
            HostingEnvironment = hostingEnvironment;
            Logger = logger;
            Repository = repository;
        }

        public SimpleBotStatus GetSimpleStatus()
        {
            var process = Process.GetCurrentProcess();

            return new SimpleBotStatus()
            {
                RamUsage = FormatHelper.FormatAsSize(process.WorkingSet64),
                InstanceType = GetInstanceType(),
                StartTime = process.StartTime,
                ThreadStatus = GetThreadStatus(process),
                ActiveCpuTime = process.TotalProcessorTime
            };
        }

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

        public void Dispose()
        {
            Repository.Dispose();
        }
    }
}