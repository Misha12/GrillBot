using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Grillbot.Extensions;
using Grillbot.Repository;
using Grillbot.Repository.Entity;
using Grillbot.Services.Config;
using Grillbot.Services.Config.Models;

namespace Grillbot.Services
{
    public class EmoteStats : IConfigChangeable, IDisposable
    {
        public Dictionary<string, EmoteStat> Counter { get; private set; }
        public HashSet<string> Changes { get; private set; }
        private Configuration Config { get; set; }
        private SemaphoreSlim Semaphore { get; set; }
        private Timer DbSyncTimer { get; set; }
        private BotLoggingService LoggingService { get; }

        public EmoteStats(Configuration configuration, BotLoggingService loggingService)
        {
            ConfigChanged(configuration);

            Counter = new Dictionary<string, EmoteStat>();
            Changes = new HashSet<string>();
            Semaphore = new SemaphoreSlim(1, 1);

            LoggingService = loggingService;

            var syncPeriod = GrillBotService.DatabaseSyncPeriod;
            DbSyncTimer = new Timer(SyncTimerCallback, null, syncPeriod, syncPeriod);
        }

        public void Init()
        {
            using (var repository = new EmoteStatsRepository(Config))
            {
                Counter = repository.GetEmoteStatistics().Result.ToDictionary(o => o.EmoteID, o => o);
            }

            LoggingService.WriteToLog($"Emote statistics loaded from database. (Rows: {Counter.Count})");
        }

        private void SyncTimerCallback(object _)
        {
            Semaphore.Wait();

            try
            {
                if (Changes.Count == 0) return;

                var changedData = Counter.Where(o => Changes.Contains(o.Key)).ToDictionary(o => o.Key, o => o.Value);
                using (var repository = new EmoteStatsRepository(Config))
                {
                    repository.UpdateEmoteStatistics(changedData).Wait();
                }

                Changes.Clear();
                LoggingService.WriteToLog($"Emote statistics was synchronized with database. (Updated {changedData.Count} records)");
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
                    .Where(o => o.Type == TagType.Emoji)
                    .Select(o => o.Value)
                    .DistinctBy(o => o.ToString())
                    .ToList();

                if (mentionedEmotes.Count == 0)
                {
                    TryIncrementUnicodeFromMessage(context.Message.Content);
                    return;
                }

                foreach (var emote in mentionedEmotes)
                {
                    if (emote is Emoji emoji)
                    {
                        IncrementCounter(emoji.Name, true);
                    }
                    else
                    {
                        var emoteId = emote.ToString();

                        if (serverEmotes.Any(o => o.ToString() == emoteId))
                            IncrementCounter(emoteId, false);
                    }
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

                if (reaction.Emote is Emoji emoji)
                {
                    IncrementCounter(emoji.Name, true);
                }
                else
                {
                    var emoteId = reaction.Emote.ToString();

                    if (serverEmotes.Any(o => o.ToString() == emoteId))
                        IncrementCounter(reaction.Emote.ToString(), false);
                }
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

                if (reaction.Emote is Emoji emoji)
                {
                    DecrementCounter(reaction.Emote.Name, true);
                }
                else
                {
                    var emoteId = reaction.Emote.ToString();

                    if (serverEmotes.Any(o => o.ToString() == emoteId))
                        DecrementCounter(emoteId, false);
                }
            }
            finally
            {
                Semaphore.Release();
            }
        }

        private void TryIncrementUnicodeFromMessage(string content)
        {
            var emojis = content
                .Split(' ')
                .Where(o => NeoSmart.Unicode.Emoji.IsEmoji(o))
                .Select(o => o.Trim());

            foreach (var emoji in emojis)
            {
                IncrementCounter(emoji, true);
            }
        }

        private void IncrementCounter(string emoteId, bool isUnicode)
        {
            if (isUnicode)
            {
                var bytes = Encoding.Unicode.GetBytes(emoteId);
                emoteId = Convert.ToBase64String(bytes);
            }

            if (!Counter.ContainsKey(emoteId))
                Counter.Add(emoteId, new EmoteStat(emoteId, isUnicode));
            else
                GetValue(emoteId).IncrementAndUpdate();

            Changes.Add(emoteId);
        }

        private void DecrementCounter(string emoteId, bool isUnicode)
        {
            if (isUnicode)
            {
                var bytes = Encoding.Unicode.GetBytes(emoteId);
                emoteId = Convert.ToBase64String(bytes);
            }

            var value = GetValue(emoteId);
            if (value == null || value.Count == 0) return;

            value.Decrement();
            Changes.Add(emoteId);
        }

        public EmoteStat GetValue(string emoteId)
        {
            return !Counter.ContainsKey(emoteId) ? null : Counter[emoteId];
        }

        public List<EmoteStat> GetAllValues(bool descOrder)
        {
            if (descOrder)
            {
                return Counter.Values
                    .OrderByDescending(o => o.Count)
                    .ThenByDescending(o => o.LastOccuredAt).ToList();
            }
            else
            {
                return Counter.Values
                    .OrderBy(o => o.Count)
                    .ThenBy(o => o.LastOccuredAt).ToList();
            }
        }

        public void ConfigChanged(Configuration newConfig)
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
