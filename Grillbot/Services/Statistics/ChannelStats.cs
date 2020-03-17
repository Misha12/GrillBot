using Discord;
using Discord.WebSocket;
using Grillbot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grillbot.Database.Entity;
using Grillbot.Services.Initiable;
using Grillbot.Database.Repository;

namespace Grillbot.Services.Statistics
{
    public class ChannelStats : IDisposable, IInitiable
    {
        public const int ChannelboardTakeTop = 10;
        public const int TokenLength = 20;

        private Dictionary<ulong, ChannelStat> Counters { get; set; }
        private static object Locker { get; } = new object();

        private Timer DbSyncTimer { get; set; }
        private HashSet<ulong> Changes { get; }
        private BotLoggingService LoggingService { get; }
        private ChannelStatsRepository Repository { get; }

        public ChannelStats(BotLoggingService loggingService, ChannelStatsRepository repository)
        {
            Changes = new HashSet<ulong>();
            Counters = new Dictionary<ulong, ChannelStat>();

            LoggingService = loggingService;
            Repository = repository;
        }

        public void Init()
        {
            Counters = Repository.GetChannelStatistics().ToDictionary(o => o.SnowflakeID, o => o);

            var syncPeriod = GrillBotService.DatabaseSyncPeriod;
            DbSyncTimer = new Timer(SyncTimerCallback, null, syncPeriod, syncPeriod);
            LoggingService.Write(LogSeverity.Info, $"Channel statistics loaded from database. (Rows: {Counters.Count})");
        }

        private void SyncTimerCallback(object _)
        {
            lock (Locker)
            {
                if (Changes.Count == 0) return;

                var itemsForUpdate = Counters.Where(o => Changes.Contains(o.Key)).Select(o => o.Value).ToList();
                Repository.UpdateChannelboard(itemsForUpdate);

                Changes.Clear();
                LoggingService.Write(LogSeverity.Info, $"Channel statistics was synchronized with database. (Updated {itemsForUpdate.Count} records)");
            }
        }

        public async Task IncrementCounterAsync(ISocketMessageChannel channel)
        {
            if (channel is IPrivateChannel) return;

            lock (Locker)
            {
                if (!Counters.ContainsKey(channel.Id))
                    Counters.Add(channel.Id, new ChannelStat() { SnowflakeID = channel.Id });

                var counterForChannel = Counters[channel.Id];

                counterForChannel.Count++;
                counterForChannel.LastMessageAt = DateTime.Now;

                Changes.Add(channel.Id);
            }
        }

        public async Task DecrementCounterAsync(ISocketMessageChannel channel)
        {
            if (channel is IPrivateChannel) return;

            lock (Locker)
            {
                if (!Counters.ContainsKey(channel.Id)) return;

                var counter = Counters[channel.Id];

                counter.Count--;
                if (counter.Count < 0)
                    counter.Count = 0;

                Changes.Add(channel.Id);
            }
        }

        public List<ChannelStat> GetAllValues()
        {
            return Counters.Values.OrderByDescending(o => o.Count).ToList();
        }

        public Tuple<int, long, DateTime> GetValue(ulong channelID)
        {
            if (!Counters.ContainsKey(channelID))
                return new Tuple<int, long, DateTime>(0, 0, DateTime.MinValue);

            var orderedData = Counters.OrderByDescending(o => o.Value.Count).ToDictionary(o => o.Key, o => o.Value);
            var position = orderedData.Keys.ToList().FindIndex(o => o == channelID) + 1;
            var channel = Counters[channelID];

            return new Tuple<int, long, DateTime>(position, channel.Count, channel.LastMessageAt);
        }

        public List<ChannelboardItem> GetChannelboardData(DiscordSocketClient client, SocketGuild guild, SocketGuildUser user)
        {
            return Counters
                .Where(o => CanUserToChannel(client, o.Key, user.Id))
                .Select(o => GetChannelboardItem(o, client))
                .OrderByDescending(o => o.Count)
                .ToList();
        }

        private ChannelboardItem GetChannelboardItem(KeyValuePair<ulong, ChannelStat> channelCountPair, DiscordSocketClient client)
        {
            if (!(client.GetChannel(channelCountPair.Key) is ISocketMessageChannel channel)) return null;

            return new ChannelboardItem()
            {
                ChannelName = channel.Name,
                Count = channelCountPair.Value.Count,
                LastMessageAt = channelCountPair.Value.LastMessageAt
            };
        }

        private bool CanUserToChannel(DiscordSocketClient client, ulong channelID, ulong userID)
        {
            var channel = client.GetChannel(channelID);
            if (channel == null) return false;
            return channel.Users.Any(o => o.Id == userID);
        }

        #region IDisposable Support

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                SyncTimerCallback(null);
                DbSyncTimer.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion

        public async Task InitAsync() { }
    }
}
