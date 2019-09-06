using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Grillbot.Extensions;
using Grillbot.Repository;
using Grillbot.Repository.Entity;
using Grillbot.Services.Config;
using Microsoft.Extensions.Configuration;

namespace Grillbot.Services
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
                Counter = repository.GetEmoteStatistics().Result.ToDictionary(o => o.EmoteID, o => o);
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
                    IncrementCounter(emoteId);
                }
            }
            finally
            {
                Semaphore.Release();
            }
        }

        public async Task IncrementFromReaction(SocketReaction reaction)
        {
            if (!(reaction.Channel is SocketGuildChannel channel)) return;

            await Semaphore.WaitAsync();
            try
            {
                var serverEmotes = channel.Guild.Emotes;

                if (serverEmotes.Contains(reaction.Emote))
                    IncrementCounter(reaction.Emote.ToString());
            }
            finally
            {
                Semaphore.Release();
            }
        }

        public async Task DecrementFromReaction(SocketReaction reaction)
        {
            if (!(reaction.Channel is SocketGuildChannel channel)) return;

            await Semaphore.WaitAsync();
            try
            {
                var serverEmotes = channel.Guild.Emotes;

                if (serverEmotes.Contains(reaction.Emote))
                    DecrementCounter(reaction.Emote.ToString());
            }
            finally
            {
                Semaphore.Release();
            }
        }

        private void IncrementCounter(string emoteId)
        {
            if (!Counter.ContainsKey(emoteId))
                Counter.Add(emoteId, new EmoteStat(emoteId));
            else
                GetValue(emoteId).IncrementAndUpdate();

            Changes.Add(emoteId);
        }

        private void DecrementCounter(string emoteId)
        {
            var value = GetValue(emoteId);
            if (value == null || value.Count == 0) return;

            value.Decrement();
            Changes.Add(emoteId);
        }

        public EmoteStat GetValue(string emoteId)
        {
            return !Counter.ContainsKey(emoteId) ? null : Counter[emoteId];
        }

        public List<EmoteStat> GetAllValues()
        {
            return Counter.Values
                .OrderByDescending(o => o.Count)
                .ThenByDescending(o => o.LastOccuredAt)
                .ToList();
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
            DbSyncTimer.Dispose();
        }
    }
}
