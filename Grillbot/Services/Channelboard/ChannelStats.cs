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
using Grillbot.Extensions.Discord;
using Grillbot.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Grillbot.Services.Channelboard
{
    public class ChannelStats : IDisposable, IInitiable
    {
        public const int ChannelboardTakeTop = 10;
        public const int TokenLength = 20;

        private Dictionary<string, ChannelStat> Counters { get; set; }
        private static object Locker { get; } = new object();

        private Timer DbSyncTimer { get; set; }
        private HashSet<string> Changes { get; }
        private ILogger<ChannelStats> Logger { get; }
        private IServiceProvider Provider { get; }
        private DiscordSocketClient Client { get; }

        public ChannelStats(IServiceProvider provider, ILogger<ChannelStats> logger, DiscordSocketClient client)
        {
            Changes = new HashSet<string>();
            Counters = new Dictionary<string, ChannelStat>();

            Logger = logger;
            Provider = provider;
            Client = client;
        }

        private ChannelStatsRepository GetRepository()
        {
            return Provider.GetService<ChannelStatsRepository>();
        }

        public void Init()
        {
            using var repository = GetRepository();
            Counters = repository.GetChannelStatistics(null).ToDictionary(o => $"{o.GuildID}|{o.ID}", o => o);

            var syncPeriod = GrillBotService.DatabaseSyncPeriod;
            DbSyncTimer = new Timer(SyncTimerCallback, null, syncPeriod, syncPeriod);
            Logger.LogInformation($"Channel statistics loaded from database. (Rows: {Counters.Count})");
        }

        private void SyncTimerCallback(object _)
        {
            lock (Locker)
            {
                if (Changes.Count == 0) return;

                var itemsForUpdate = Counters.Where(o => Changes.Contains(o.Key)).Select(o => o.Value).ToList();
                using var repository = GetRepository();
                repository.UpdateChannelboard(itemsForUpdate);

                Changes.Clear();
                Logger.LogInformation($"Channel statistics was synchronized with database. (Updated {itemsForUpdate.Count} records.)");
            }
        }

        public async Task IncrementCounterAsync(SocketGuildChannel channel)
        {
            if (channel is IPrivateChannel) return;

            lock (Locker)
            {
                var key = $"{channel.Guild.Id}|{channel.Id}";
                if (!Counters.ContainsKey(key))
                {
                    var item = new ChannelStat()
                    {
                        GuildIDSnowflake = channel.Guild.Id,
                        SnowflakeID = channel.Id
                    };

                    Counters.Add(key, item);
                }

                var counterForChannel = Counters[key];

                counterForChannel.Count++;
                counterForChannel.LastMessageAt = DateTime.Now;

                Changes.Add(key);
            }
        }

        public async Task DecrementCounterAsync(SocketGuildChannel channel)
        {
            if (channel is IPrivateChannel) return;

            lock (Locker)
            {
                var key = $"{channel.Guild.Id}|{channel.Id}";
                if (!Counters.ContainsKey(key)) return;

                var counter = Counters[key];

                counter.Count--;
                if (counter.Count < 0)
                    counter.Count = 0;

                Changes.Add(key);
            }
        }

        public List<ChannelStat> GetAllValues(SocketGuild guild)
        {
            return Counters.Values
                .Where(o => o.GuildIDSnowflake == guild.Id)
                .OrderByDescending(o => o.Count)
                .ToList();
        }

        public async Task<Tuple<int, long, DateTime>> GetValueAsync(SocketGuild guild, ulong channelID, SocketUser user)
        {
            var key = $"{guild.Id}|{channelID}";

            if (!Counters.ContainsKey(key))
                return new Tuple<int, long, DateTime>(0, 0, DateTime.MinValue);

            var channel = Counters[key];

            if (!(await CanUserToChannelAsync(guild.Id, channelID, user.Id)))
                return null;

            var position = Counters
                .OrderByDescending(o => o.Value.Count)
                .Select(o => o.Key)
                .ToList()
                .FindIndex(o => o == key) + 1;

            return new Tuple<int, long, DateTime>(position, channel.Count, channel.LastMessageAt);
        }

        public async Task<List<ChannelboardItem>> GetChannelboardDataAsync(SocketGuild guild, SocketUser user)
        {
            var result = new List<ChannelboardItem>();

            foreach(var stat in Counters.Values.Where(o => o.GuildIDSnowflake == guild.Id))
            {
                if (!(await CanUserToChannelAsync(stat.GuildIDSnowflake, stat.SnowflakeID, user.Id)))
                    continue;

                var item = GetChannelboardItem(stat);

                if (item == null)
                    continue;

                result.Add(item);
            }

            return result
                .OrderByDescending(o => o.Count)
                .ThenByDescending(o => o.LastMessageAt)
                .ToList();
        }

        private ChannelboardItem GetChannelboardItem(ChannelStat channelStat)
        {
            var guild = Client.GetGuild(channelStat.GuildIDSnowflake);
            if (guild == null) return null;

            var channel = guild.GetTextChannel(channelStat.SnowflakeID);
            if (channel == null) return null;

            return new ChannelboardItem()
            {
                ChannelName = channel.Name,
                Count = channelStat.Count,
                LastMessageAt = channelStat.LastMessageAt
            };
        }

        public async Task<bool> CanUserToChannelAsync(ulong guildID, ulong channelID, ulong userID)
        {
            var guild = Client.GetGuild(guildID);
            if (guild == null)
                return false;

            await guild.SyncGuildAsync();

            var channel = guild.GetChannel(channelID);
            if (channel == null)
                return false;

            return channel.Users.Any(o => o.Id == userID);
        }

        public async Task<List<string>> CleanOldChannels(SocketGuild guild)
        {
            await guild.SyncGuildAsync();

            lock (Locker)
            {
                var removed = new List<string>();

                var stats = Counters.Values
                    .Where(o => o.GuildIDSnowflake == guild.Id)
                    .ToList();

                using var repository = GetRepository();
                foreach (var channel in stats)
                {
                    var dcChannel = guild.GetChannel(channel.SnowflakeID);

                    if (dcChannel == null)
                    {
                        removed.Add($"Kanál {channel.ID} s počtem zpráv {FormatHelper.FormatWithSpaces(channel.Count)} byl smazán.");
                        repository.RemoveChannel(channel);
                        Counters.Remove($"{channel.GuildID}|{channel.ID}");
                    }
                }

                return removed;
            }
        }

        public void Dispose()
        {
            SyncTimerCallback(null);
            DbSyncTimer?.Dispose();
        }

        public async Task InitAsync() { }
    }
}
