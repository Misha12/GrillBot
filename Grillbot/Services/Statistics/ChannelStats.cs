using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Grillbot.Helpers;
using Grillbot.Models;
using Grillbot.Database;
using Grillbot.Services.Config.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grillbot.Database.Entity;
using Microsoft.Extensions.Options;
using Grillbot.Services.Initiable;

namespace Grillbot.Services.Statistics
{
    public class ChannelStats : IDisposable, IInitiable
    {
        public const int ChannelboardTakeTop = 10;
        public const int TokenLength = 20;

        private Dictionary<ulong, ChannelStat> Counters { get; }
        private static object Locker { get; } = new object();

        private Configuration Config { get; }
        private Timer DbSyncTimer { get; set; }
        private HashSet<ulong> Changes { get; }
        private List<ChannelboardWebToken> WebTokens { get; }
        private BotLoggingService LoggingService { get; }

        public ChannelStats(IOptions<Configuration> config, BotLoggingService loggingService)
        {
            Changes = new HashSet<ulong>();
            WebTokens = new List<ChannelboardWebToken>();
            Counters = new Dictionary<ulong, ChannelStat>();

            LoggingService = loggingService;
            Config = config.Value;
        }

        public void Init()
        {
            using (var repository = new GrillBotRepository(Config))
            {
                var data = repository.ChannelStats.GetChannelStatistics();

                foreach (var stat in data)
                {
                    Counters.Add(stat.SnowflakeID, stat);
                }
            }

            var syncPeriod = GrillBotService.DatabaseSyncPeriod;
            DbSyncTimer = new Timer(SyncTimerCallback, null, syncPeriod, syncPeriod);
            LoggingService.Write($"Channel statistics loaded from database. (Rows: {Counters.Count})");
        }

        private void SyncTimerCallback(object _)
        {
            lock (Locker)
            {
                CleanInvalidWebTokens();

                if (Changes.Count == 0) return;

                var itemsForUpdate = Counters.Where(o => Changes.Contains(o.Key)).Select(o => o.Value).ToList();
                using (var repository = new GrillBotRepository(Config))
                {
                    repository.ChannelStats.UpdateChannelboard(itemsForUpdate);
                }

                Changes.Clear();
                LoggingService.Write($"Channel statistics was synchronized with database. (Updated {itemsForUpdate.Count} records)");
            }
        }

        private void CleanInvalidWebTokens()
        {
            if (WebTokens.Count == 0) return;

            WebTokens.RemoveAll(o => !o.IsValid());
            LoggingService.Write("Cleared invalid web tokens.");
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

        public ChannelboardWebToken CreateWebToken(SocketCommandContext context)
        {
            using(var repository = new GrillBotRepository(Config))
            {
                var config = repository.Config.FindConfig(context.Guild.Id, "", "channelboardweb").GetData<ChannelboardConfig>();

                var tokenValidFor = config.GetTokenValidTime();
                var token = StringHelper.CreateRandomString(TokenLength);
                var rawUrl = config.WebUrl;

                var webToken = new ChannelboardWebToken(token, context.Message.Author.Id, tokenValidFor, rawUrl);
                WebTokens.Add(webToken);

                return webToken;
            }
        }

        public bool ExistsWebToken(string token) => WebTokens.Any(o => o.Token == token);

        public List<ChannelboardItem> GetChannelboardData(string token, DiscordSocketClient client, out ChannelboardWebToken webToken)
        {
            var tokenData = WebTokens.Find(o => o.Token == token);

            webToken = tokenData;
            return Counters
                .Where(o => CanUserToChannel(client, o.Key, tokenData.UserID))
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

        public int GetActiveWebTokensCount() => WebTokens.Count(o => o.IsValid());

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
