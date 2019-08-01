using Discord.Commands;
using Discord.WebSocket;
using Grillbot.Helpers;
using Grillbot.Repository;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Grillbot.Services.Statistics
{
    public class ChannelStats : IConfigChangeable, IDisposable
    {
        public Dictionary<ulong, long> Counter { get; private set; }
        private IConfiguration Config { get; set; }
        private Timer DbSyncTimer { get; set; }
        private List<ulong> Changes { get; set; }
        private SemaphoreSlim Semaphore { get; set; }
        private List<ChannelboardWebToken> WebTokens { get; set; }

        public ChannelStats(IConfiguration config)
        {
            Counter = new Dictionary<ulong, long>();
            Changes = new List<ulong>();
            WebTokens = new List<ChannelboardWebToken>();

            Config = config;
            Semaphore = new SemaphoreSlim(1, 1);
        }

        public async Task Init()
        {
            using (var repository = new ChannelStatsRepository(Config))
            {
                var data = await repository.GetStatistics();
                Counter = data.ToDictionary(o => o.Item1, o => o.Item2);
            }

            Reload(Config);
            await Console.Out.WriteLineAsync($"{DateTime.Now.ToLongTimeString()} BOT\tChannel statistics loaded from database. (Rows: {Counter.Count})");
        }

        private void Reload(IConfiguration config)
        {
            var syncTimerConfig = Convert.ToInt32(config["Leaderboards:SyncWithDBSecs"]) * 1000;
            DbSyncTimer = new Timer(SyncTimerCallback, null, syncTimerConfig, syncTimerConfig);
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
                if (Changes.Count == 0) return;
                var forUpdate = Counter.Where(o => Changes.Contains(o.Key)).ToDictionary(o => o.Key, o => o.Value);

                using (var repository = new ChannelStatsRepository(Config))
                {
                    repository.UpdateStatistics(forUpdate).Wait();
                }

                Changes.Clear();
                Console.WriteLine($"{DateTime.Now.ToLongTimeString()} BOT\tChannel statistics was synchronized with database. (Updated {forUpdate.Count} records)");

                CleanInvalidWebTokens();
            }
            finally
            {
                Semaphore.Release();
            }
        }

        private void CleanInvalidWebTokens()
        {
            WebTokens.RemoveAll(o => !o.IsValid());
            Console.WriteLine($"{DateTime.Now.ToLongTimeString()} BOT\tCleared invalid web tokens.");
        }

        public async Task IncrementCounter(ulong channelID)
        {
            await Semaphore.WaitAsync();

            try
            {
                if (!Counter.ContainsKey(channelID))
                    Counter.Add(channelID, 1);
                else
                    Counter[channelID]++;

                if (!Changes.Contains(channelID))
                    Changes.Add(channelID);
            }
            finally
            {
                Semaphore.Release();
            }
        }

        public async Task DecrementCounter(ulong channelID)
        {
            await Semaphore.WaitAsync();

            try
            {
                if (!Counter.ContainsKey(channelID)) return;

                Counter[channelID]--;

                if (Counter[channelID] < 0)
                    Counter[channelID] = 0;

                if (!Changes.Contains(channelID))
                    Changes.Add(channelID);
            }
            finally
            {
                Semaphore.Release();
            }
        }

        public Tuple<int, long> GetValue(ulong channelID)
        {
            if (!Counter.ContainsKey(channelID))
                return new Tuple<int, long>(0, 0);

            var orderedData = Counter.OrderByDescending(o => o.Value).ToDictionary(o => o.Key, o => o.Value);
            var keyPosition = orderedData.Keys.ToList().FindIndex(o => o == channelID) + 1;
            return new Tuple<int, long>(keyPosition, orderedData[channelID]);
        }

        public ChannelboardWebToken CreateWebToken(SocketCommandContext context)
        {
            var config = Config.GetSection("MethodsConfig:Channelboard:Web");

            var tokenValidFor = TimeSpan.FromMinutes(Convert.ToInt32(config["TokenValidMins"]));
            var tokenLength = Convert.ToInt32(config["TokenLength"]);
            var token = StringHelper.CreateRandomString(tokenLength);
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
            var channel = client.GetChannel(channelCountPair.Key) as ISocketMessageChannel;

            return new ChannelboardItem()
            {
                ChannelName = channel.Name,
                Count = channelCountPair.Value
            };
        }

        private bool CanUserToChannel(DiscordSocketClient client, ulong channelID, ulong userID)
        {
            var channel = client.GetChannel(channelID);
            return channel.Users.Any(o => o.Id == userID);
        }

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
