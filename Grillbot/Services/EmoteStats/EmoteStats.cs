using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Grillbot.Extensions;
using Grillbot.Repository;
using Grillbot.Repository.Entity;
using Grillbot.Services.Config;
using Microsoft.Extensions.Configuration;

namespace Grillbot.Services.EmoteStats
{
    public class EmoteStats : IConfigChangeable, IDisposable
    {
        public Dictionary<string, EmoteStat> Counter { get; private set; }
        public HashSet<string> Changes { get; private set; }
        private IConfiguration Config { get; set; }
        private SemaphoreSlim Semaphore { get; set; }
        private Timer DbSyncTimer { get; set; }

        public EmoteStats(IConfiguration configuration)
        {
            ConfigChanged(configuration);

            Counter = new Dictionary<string, EmoteStat>();
            Changes = new HashSet<string>();
            Semaphore = new SemaphoreSlim(1, 1);

            var syncPeriod = GrillBotService.DatabaseSyncPeriod;
            DbSyncTimer = new Timer(SyncTimerCallback, null, syncPeriod, syncPeriod);
        }

        public void Init()
        {
            using(var repository = new EmoteStatsRepository(Config))
            {
                var data = repository.GetEmoteStatistics().Result;
                Counter = data.ToDictionary(o => o.EmoteID, o => o);
            }

            Console.WriteLine($"{DateTime.Now.ToLongTimeString()} BOT\tEmote statistics loaded from database. (Rows: {Counter.Count})");
        }

        private void SyncTimerCallback(object _)
        {
            Semaphore.Wait();

            try
            {
                if (Changes.Count == 0) return;

                var changedData = Counter.Where(o => Changes.Contains(o.Key)).ToDictionary(o => o.Key, o => o.Value);
                using(var repository = new EmoteStatsRepository(Config))
                {
                    repository.UpdateEmoteStatistics(changedData).Wait();
                }

                Changes.Clear();
                Console.WriteLine($"{DateTime.Now.ToLongTimeString()} BOT\tEmote statistics was synchronized with database. (Updated {changedData.Count} records)");
            }
            finally
            {
                Semaphore.Release();
            }
        }

        public async Task AnylyzeMessageAndIncrementValuesAsync(SocketCommandContext context)
        {
            if (context.Guild == null) return;

            await Semaphore.WaitAsync();
            try
            {
                var serverEmotes = context.Guild.Emotes;
                var mentionedEmotes = context.Message.Tags
                    .Where(o => o.Type == TagType.Emoji && serverEmotes.Any(x => x.Id == o.Key))
                    .DistinctBy(o => o.ToString())
                    .Select(o => o.Value.ToString())
                    .ToList();

                foreach(var emoteId in mentionedEmotes)
                {
                    if (!Counter.ContainsKey(emoteId))
                        Counter.Add(emoteId, new EmoteStat(emoteId));
                    else
                        Counter[emoteId].IncrementAndUpdate();

                    Changes.Add(emoteId);
                }
            }
            finally
            {
                Semaphore.Release();
            }
        }

        public EmoteStat GetValue(string emoteId)
        {
            return !Counter.ContainsKey(emoteId) ? null : Counter[emoteId];
        }

        public List<EmoteStat> GetAllValues()
        {
            return Counter.Values.OrderByDescending(o => o.LastOccuredAt).ToList();
        }

        public void ConfigChanged(IConfiguration newConfig)
        {
            Config = newConfig;
        }

        public void Dispose()
        {
            SyncTimerCallback(null);

            Counter.Clear();
            Changes.Clear();
            Semaphore.Dispose();
        }
    }
}
