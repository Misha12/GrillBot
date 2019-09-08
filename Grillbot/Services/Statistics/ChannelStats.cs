using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Grillbot.Helpers;
using Grillbot.Models;
using Grillbot.Repository;
using Grillbot.Services.Config;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grillbot.Services.Statistics
{
    public class ChannelStats : IConfigChangeable, IDisposable
    {
        public const int ChannelboardTakeTop = 10;
        public const int TokenLength = 20;

        public Dictionary<ulong, long> Counter { get; private set; }
        private IConfiguration Config { get; set; }
        private Timer DbSyncTimer { get; set; }
        private HashSet<ulong> Changes { get; set; }
        private SemaphoreSlim Semaphore { get; set; }
        private List<ChannelboardWebToken> WebTokens { get; set; }
        private Dictionary<ulong, DateTime> LastMessagesAt { get; set; }

        public ChannelStats(IConfiguration config)
        {
            Counter = new Dictionary<ulong, long>();
            Changes = new HashSet<ulong>();
            WebTokens = new List<ChannelboardWebToken>();
            LastMessagesAt = new Dictionary<ulong, DateTime>();

            Reload(config);
            Semaphore = new SemaphoreSlim(1, 1);
        }

        public void Init()
        {
            using (var repository = new ChannelStatsRepository(Config))
            {
                var data = repository.GetChannelStatistics().Result;

                Counter = data.ToDictionary(o => o.SnowflakeID, o => o.Count);
                LastMessagesAt = data.ToDictionary(o => o.SnowflakeID, o => o.LastMessageAt);
            }

            var syncPeriod = GrillBotService.DatabaseSyncPeriod;
            DbSyncTimer = new Timer(SyncTimerCallback, null, syncPeriod, syncPeriod);

            Reload(Config);
            Console.WriteLine($"{DateTime.Now.ToLongTimeString()} BOT\tChannel statistics loaded from database. (Rows: {Counter.Count})");
        }

        private void Reload(IConfiguration config)
        {
            Config = config;
        }

        public void ConfigChanged(IConfiguration newConfig)
        {
            Reload(newConfig);
        }

        private void SyncTimerCallback(object _)
        {
            Semaphore.Wait();

            try
            {
                CleanInvalidWebTokens();

                if (Changes.Count == 0) return;
                var forUpdate = Counter.Where(o => Changes.Contains(o.Key)).ToDictionary(o => o.Key, o => o.Value);
                var lastMessageDates = LastMessagesAt.Where(o => Changes.Contains(o.Key)).ToDictionary(o => o.Key, o => o.Value);

                using (var repository = new ChannelStatsRepository(Config))
                {
                    repository.UpdateChannelboardStatisticsAsync(forUpdate, lastMessageDates).Wait();
                }

                Changes.Clear();
                Console.WriteLine($"{DateTime.Now.ToLongTimeString()} BOT\tChannel statistics was synchronized with database. (Updated {forUpdate.Count} records)");
            }
            finally
            {
                Semaphore.Release();
            }
        }

        private void CleanInvalidWebTokens()
        {
            if (WebTokens.Count == 0) return;

            WebTokens.RemoveAll(o => !o.IsValid());
            Console.WriteLine($"{DateTime.Now.ToLongTimeString()} BOT\tCleared invalid web tokens.");
        }

        public async Task IncrementCounterAsync(ISocketMessageChannel channel)
        {
            if (channel is IPrivateChannel) return;

            await Semaphore.WaitAsync();
            try
            {
                if (!Counter.ContainsKey(channel.Id))
                    Counter.Add(channel.Id, 1);
                else
                    Counter[channel.Id]++;

                Changes.Add(channel.Id);

                if (!LastMessagesAt.ContainsKey(channel.Id))
                    LastMessagesAt.Add(channel.Id, DateTime.Now);
                else
                    LastMessagesAt[channel.Id] = DateTime.Now;
            }
            finally
            {
                Semaphore.Release();
            }
        }

        public async Task DecrementCounterAsync(ISocketMessageChannel channel)
        {
            if (channel is IPrivateChannel) return;

            await Semaphore.WaitAsync();

            try
            {
                if (!Counter.ContainsKey(channel.Id)) return;

                Counter[channel.Id]--;

                if (Counter[channel.Id] < 0)
                    Counter[channel.Id] = 0;

                Changes.Add(channel.Id);
            }
            finally
            {
                Semaphore.Release();
            }
        }

        public Tuple<int, long, DateTime> GetValue(ulong channelID)
        {
            if (!Counter.ContainsKey(channelID))
                return new Tuple<int, long, DateTime>(0, 0, DateTime.MinValue);

            var orderedData = Counter.OrderByDescending(o => o.Value).ToDictionary(o => o.Key, o => o.Value);
            var keyPosition = orderedData.Keys.ToList().FindIndex(o => o == channelID) + 1;
            var lastMessageAt = LastMessagesAt.ContainsKey(channelID) ? LastMessagesAt[channelID] : DateTime.MinValue;
            return new Tuple<int, long, DateTime>(keyPosition, orderedData[channelID], lastMessageAt);
        }

        public ChannelboardWebToken CreateWebToken(SocketCommandContext context)
        {
            var config = Config.GetSection("MethodsConfig:Channelboard:Web");

            var tokenValidFor = TimeSpan.FromMinutes(Convert.ToInt32(config["TokenValidMins"]));
            var token = StringHelper.CreateRandomString(TokenLength);
            var rawUrl = config["Url"];

            var webToken = new ChannelboardWebToken(token, context.Message.Author.Id, tokenValidFor, rawUrl);
            WebTokens.Add(webToken);

            return webToken;
        }

        public bool ExistsWebToken(string token) => WebTokens.Any(o => o.Token == token);

        public List<ChannelboardItem> GetChannelboardData(string token, DiscordSocketClient client, out ChannelboardWebToken webToken)
        {
            var tokenData = WebTokens.Find(o => o.Token == token);

            webToken = tokenData;
            return Counter
                .Where(o => CanUserToChannel(client, o.Key, tokenData.UserID))
                .Select(o => GetChannelboardItem(o, client))
                .OrderByDescending(o => o.Count)
                .ToList();
        }

        private ChannelboardItem GetChannelboardItem(KeyValuePair<ulong, long> channelCountPair, DiscordSocketClient client)
        {
            if (!(client.GetChannel(channelCountPair.Key) is ISocketMessageChannel channel)) return null;
            var lastMessageAt = LastMessagesAt.ContainsKey(channel.Id) ? LastMessagesAt[channel.Id] : DateTime.MinValue;

            return new ChannelboardItem()
            {
                ChannelName = channel.Name,
                Count = channelCountPair.Value,
                LastMessageAt = lastMessageAt
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
                DbSyncTimer.Dispose();
                Semaphore.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
