using Discord.WebSocket;
using Grillbot.Database.Repository;
using Grillbot.Extensions;
using Grillbot.Helpers;
using Grillbot.Models.BotStatus;
using Grillbot.Models.Internal;
using Grillbot.Services.MessageCache;
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
        private DiscordSocketClient Client { get; }
        private IMessageCache MessageCache { get; }

        public BotStatusService(IWebHostEnvironment hostingEnvironment, Logger.Logger logger, BotDbRepository repository, DiscordSocketClient client,
            IMessageCache messageCache)
        {
            HostingEnvironment = hostingEnvironment;
            Logger = logger;
            Repository = repository;
            Client = client;
            MessageCache = messageCache;
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

            return $"{process.Threads.Count.FormatWithSpaces()} ({sleepCount.FormatWithSpaces()} spí)";
        }

        public async Task<Dictionary<string, Tuple<int, long>>> GetDbReport()
        {
            return await Repository.GetTableRowsCount().ConfigureAwait(false);
        }

        public List<CacheStatus> GetCacheStatus()
        {
            var result = new List<CacheStatus>();

            foreach(var channel in Client.Guilds.SelectMany(o => o.TextChannels))
            {
                var messageCache = MessageCache.GetFromChannel(channel.Id);

                var item = new CacheStatus(channel)
                {
                    InternalCacheCount = channel.CachedMessages.Count,
                    MessageCacheCount = messageCache.Count(),
                };

                result.Add(item);
            }

            return result;
        }

        public void Dispose()
        {
            Repository.Dispose();
        }
    }
}