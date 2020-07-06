using Discord;
using Discord.WebSocket;
using Grillbot.Database.Repository;
using Grillbot.Extensions;
using Grillbot.Models.BotStatus;
using Grillbot.Models.Internal;
using Grillbot.Services.MessageCache;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
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
        private Logger.Logger Logger { get; }
        private DiscordSocketClient Client { get; }
        private IMessageCache MessageCache { get; }
        private IServiceProvider Provider { get; }

        public List<SocketMessage> RunningCommands { get; }

        public BotStatusService(IWebHostEnvironment hostingEnvironment, Logger.Logger logger, IServiceProvider provider, DiscordSocketClient client,
            IMessageCache messageCache)
        {
            HostingEnvironment = hostingEnvironment;
            Logger = logger;
            Client = client;
            MessageCache = messageCache;
            Provider = provider;

            RunningCommands = new List<SocketMessage>();
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
            using var scope = Provider.CreateScope();
            using var repository = scope.ServiceProvider.GetService<BotDbRepository>();

            return await repository.GetTableRowsCount().ConfigureAwait(false);
        }

        public List<CacheStatus> GetCacheStatus()
        {
            var result = new List<CacheStatus>();

            var channels = Client.Guilds
                .OrderBy(o => o.Name)
                .SelectMany(o => o.TextChannels)
                .OrderBy(o => o.Name);

            foreach (var channel in channels)
            {
                var messageCache = MessageCache.GetFromChannel(channel.Id);

                result.Add(new CacheStatus(channel)
                {
                    InternalCacheCount = channel.CachedMessages.Count,
                    MessageCacheCount = messageCache.Count(),
                });
            }

            return result;
        }

        public List<CacheStatus> GetCacheStatus(SocketGuild guild)
        {
            var result = new List<CacheStatus>();

            var channels = guild.TextChannels
                .OrderBy(o => o.Name);

            foreach(var channel in channels)
            {
                var messageCache = MessageCache.GetFromChannel(channel.Id);

                result.Add(new CacheStatus(channel)
                {
                    InternalCacheCount = channel.CachedMessages.Count,
                    MessageCacheCount = messageCache.Count()
                });
            }

            return result;
        }

        public CacheStatus GetCacheStatus(SocketGuild guild, IChannel channel)
        {
            var messageCache = MessageCache.GetFromChannel(channel.Id);
            var guildChannel = guild.GetTextChannel(channel.Id);

            return new CacheStatus(guildChannel)
            {
                MessageCacheCount = messageCache.Count(),
                InternalCacheCount = guildChannel.CachedMessages.Count
            };
        }
    }
}