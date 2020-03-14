using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Grillbot.Database;
using Grillbot.Database.Entity;
using Grillbot.Database.Repository;
using Grillbot.Extensions;
using Grillbot.Extensions.Discord;
using Grillbot.Models;
using Grillbot.Services.Config.Models;
using Grillbot.Services.Initiable;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Grillbot.Services.Statistics
{
    public class EmoteStats : IDisposable, IInitiable
    {
        private Dictionary<string, EmoteStat> Counter { get; set; }
        private HashSet<string> Changes { get; }
        private Configuration Config { get; }
        private Timer DbSyncTimer { get; }
        private BotLoggingService LoggingService { get; }
        private static object Locker { get; } = new object();
        private EmoteStatsRepository Repository { get; }

        public EmoteStats(IOptions<Configuration> config, BotLoggingService loggingService, EmoteStatsRepository repository)
        {
            Config = config.Value;

            Counter = new Dictionary<string, EmoteStat>();
            Changes = new HashSet<string>();

            LoggingService = loggingService;
            Repository = repository;

            var syncPeriod = GrillBotService.DatabaseSyncPeriod;
            DbSyncTimer = new Timer(SyncTimerCallback, null, syncPeriod, syncPeriod);
        }

        public void Init()
        {
            Counter = Repository.GetEmoteStatistics().ToDictionary(o => o.EmoteID, o => o);
            LoggingService.Write($"Emote statistics loaded from database. (Rows: {Counter.Count})");
        }

        private void SyncTimerCallback(object _)
        {
            lock (Locker)
            {
                if (Changes.Count == 0) return;

                var dataForUpdate = Counter.Where(o => Changes.Contains(o.Key)).ToDictionary(o => o.Key, o => o.Value);
                Repository.UpdateEmoteStatistics(dataForUpdate);

                Changes.Clear();
                LoggingService.Write($"Emote statistics was synchronized with database. (Updated {dataForUpdate.Count} records)");
            }
        }

        public async Task AnylyzeMessageAndIncrementValuesAsync(SocketCommandContext context)
        {
            lock (Locker)
            {
                if (context.Guild == null) return;

                var mentionedEmotes = context.Message.Tags
                    .Where(o => o.Type == TagType.Emoji)
                    .Select(o => o.Value)
                    .DistinctBy(o => o.ToString())
                    .ToList();

                if (mentionedEmotes.Count == 0)
                {
                    TryIncrementUnicodeFromMessage(context.Message.Content, context.Guild);
                    return;
                }

                foreach (var emote in mentionedEmotes)
                {
                    if (emote is Emoji emoji)
                    {
                        IncrementCounter(emoji.Name, true, context.Guild);
                    }
                    else
                    {
                        var emoteId = emote.ToString();
                        var serverEmotes = context.Guild.Emotes;

                        if (serverEmotes.Any(o => o.ToString() == emoteId))
                            IncrementCounter(emoteId, false, context.Guild);
                    }
                }
            }
        }

        public async Task IncrementFromReaction(SocketReaction reaction)
        {
            lock (Locker)
            {
                if (!(reaction.Channel is SocketGuildChannel channel)) return;

                var serverEmotes = channel.Guild.Emotes;

                if (reaction.Emote is Emoji emoji)
                {
                    IncrementCounter(emoji.Name, true, channel.Guild);
                }
                else
                {
                    var emoteId = reaction.Emote.ToString();

                    if (serverEmotes.Any(o => o.ToString() == emoteId))
                        IncrementCounter(reaction.Emote.ToString(), false, channel.Guild);
                }
            }
        }

        public async Task DecrementFromReaction(SocketReaction reaction)
        {
            lock (Locker)
            {
                if (!(reaction.Channel is SocketGuildChannel channel)) return;

                if (reaction.Emote is Emoji emoji)
                {
                    DecrementCounter(reaction.Emote.Name, true);
                }
                else
                {
                    var emoteId = reaction.Emote.ToString();
                    var serverEmotes = channel.Guild.Emotes;

                    if (serverEmotes.Any(o => o.ToString() == emoteId))
                        DecrementCounter(emoteId, false);
                }
            }
        }

        private void TryIncrementUnicodeFromMessage(string content, SocketGuild guild)
        {
            var emojis = content
                .Split(' ')
                .Where(o => NeoSmart.Unicode.Emoji.IsEmoji(o))
                .Select(o => o.Trim());

            foreach (var emoji in emojis)
            {
                IncrementCounter(emoji, true, guild);
            }
        }

        private void IncrementCounter(string emoteId, bool isUnicode, SocketGuild guild)
        {
            if (isUnicode)
            {
                var bytes = Encoding.Unicode.GetBytes(emoteId);
                emoteId = Convert.ToBase64String(bytes);
            }

            if (!Counter.ContainsKey(emoteId))
                Counter.Add(emoteId, new EmoteStat(emoteId, isUnicode, guild.Id));
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

        public List<EmoteStat> GetAllValues(bool descOrder, ulong guildID, bool excludeUnicode)
        {
            var query = Counter.Values.Where(o => o.GuildIDSnowflake == guildID);

            if (excludeUnicode)
                query = query.Where(o => !o.IsUnicode);

            if (descOrder)
                return query.OrderByDescending(o => o.Count).ThenByDescending(o => o.LastOccuredAt).ToList();
            else
                return query.OrderBy(o => o.Count).ThenBy(o => o.LastOccuredAt).ToList();
        }

        public void Dispose()
        {
            SyncTimerCallback(null);

            Counter.Clear();
            Changes.Clear();
            DbSyncTimer.Dispose();
        }

        public List<EmoteMergeListItem> GetMergeList(SocketGuild guild)
        {
            var emotes = GetAllValues(true, guild.Id, true);
            var data = guild.Emotes.Select(o => new EmoteMergeListItem() { Emote = o }).ToList();

            foreach (var emote in emotes)
            {
                var emoteData = Emote.Parse(emote.GetRealId());
                var serverEmote = data.Find(o => o.Emote.Id == emoteData.Id);

                if (serverEmote != null && emoteData.Name != serverEmote.Emote.Name)
                {
                    serverEmote.Emotes.Add(emoteData.ToString(), emote.Count);
                }
            }

            return data.FindAll(o => o.Emotes.Count > 0);
        }

        public async Task MergeEmotesAsync(SocketGuild guild)
        {
            lock (Locker)
            {
                foreach (var item in GetMergeList(guild))
                {
                    if (!Counter.ContainsKey(item.MergeTo))
                        Counter.Add(item.MergeTo, new EmoteStat() { EmoteID = item.MergeTo, LastOccuredAt = DateTime.Now });

                    var emote = Counter[item.MergeTo];

                    foreach (var source in item.Emotes)
                    {
                        emote.Count += source.Value;
                        Repository.RemoveEmote(source.Key);
                        Counter.Remove(source.Key);
                    }

                    Changes.Add(emote.EmoteID);
                }
            }
        }

        public async Task<List<string>> CleanOldEmotesAsync(SocketGuild guild)
        {
            await guild.SyncGuildAsync().ConfigureAwait(false);

            lock (Locker)
            {
                var removed = new List<string>();

                foreach (var emote in Counter.Values.Where(o => !o.IsUnicode).ToList())
                {
                    var parsedEmote = Emote.Parse(emote.GetRealId());

                    if (!guild.Emotes.Any(o => o.Id == parsedEmote.Id))
                    {
                        removed.Add($"Mažu starý emote {parsedEmote.Name} ({parsedEmote.Id})");
                        Repository.RemoveEmote(emote.GetRealId());
                        Counter.Remove(emote.GetRealId());
                    }
                }

                return removed;
            }
        }

        public async Task InitAsync() { }
    }
}
