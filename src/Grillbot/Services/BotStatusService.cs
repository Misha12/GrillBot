using Grillbot.Database;
using Grillbot.Database.Entity.AuditLog;
using Grillbot.Enums;
using Grillbot.Extensions;
using Grillbot.Models.Audit;
using Grillbot.Models.BotStatus;
using Grillbot.Services.Statistics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Services
{
    public class BotStatusService
    {
        private IWebHostEnvironment HostingEnvironment { get; }
        private IGrillBotRepository GrillBotRepository { get; }
        private InternalStatistics InternalStatistics { get; }

        public BotStatusService(IWebHostEnvironment hostingEnvironment, IGrillBotRepository grillBotRepository,
            InternalStatistics internalStatistics)
        {
            HostingEnvironment = hostingEnvironment;
            GrillBotRepository = grillBotRepository;
            InternalStatistics = internalStatistics;
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

        public async Task<Dictionary<string, Tuple<ulong, int>>> GetCommandsReportAsync()
        {
            var calledCommands = InternalStatistics.GetCommands();
            var auditLogs = await GrillBotRepository.AuditLogs.GetAuditLogsByType(AuditLogType.Command)
                .Select(o => new AuditLogItem() { JsonData = o.JsonData })
                .AsNoTracking()
                .ToListAsync();

            var statsFromAuditLogs = auditLogs
                .Select(o => o.GetData<CommandAuditData>())
                .Select(o => $"{o.Group} {o.CommandName}".Trim())
                .Where(o => calledCommands.ContainsKey(o))
                .GroupBy(o => o)
                .ToDictionary(o => o.Key, o => o.Count());

            var completeCommandStats = new Dictionary<string, Tuple<ulong, int>>();

            foreach (var pair in calledCommands)
            {
                var count = statsFromAuditLogs.ContainsKey(pair.Key) ? statsFromAuditLogs[pair.Key] : 0;
                completeCommandStats.Add(pair.Key, Tuple.Create(pair.Value, count));
            }

            return completeCommandStats
                .OrderByDescending(o => o.Value.Item1)
                .ThenByDescending(o => o.Value.Item2)
                .ThenBy(o => o.Key)
                .ToDictionary(o => o.Key, o => o.Value);
        }
    }
}