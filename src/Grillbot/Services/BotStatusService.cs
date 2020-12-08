using Grillbot.Database;
using Grillbot.Extensions;
using Grillbot.Models.BotStatus;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Grillbot.Services
{
    public class BotStatusService
    {
        private IWebHostEnvironment HostingEnvironment { get; }
        private IGrillBotRepository GrillBotRepository { get; }

        public BotStatusService(IWebHostEnvironment hostingEnvironment, IGrillBotRepository grillBotRepository)
        {
            HostingEnvironment = hostingEnvironment;
            GrillBotRepository = grillBotRepository;
        }

        public SimpleBotStatus GetSimpleStatus()
        {
            var process = Process.GetCurrentProcess();

            return new SimpleBotStatus()
            {
                RamUsage = process.WorkingSet64.FormatAsSize(),
                InstanceType = GetInstanceType(),
                StartTime = process.StartTime,
                ThreadStatus = GetThreadStatus(process),
                ActiveCpuTime = process.TotalProcessorTime
            };
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

            return $"{process.Threads.Count.FormatWithSpaces()} ({sleepCount.FormatWithSpaces()} spí)";
        }

        public async Task<Dictionary<string, Tuple<int, long>>> GetDbReport()
        {
            return await GrillBotRepository.BotDbRepository.GetTableRowsCountAsync();
        }
    }
}