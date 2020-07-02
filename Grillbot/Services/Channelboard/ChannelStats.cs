using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grillbot.Database.Repository;
using Grillbot.Extensions.Discord;
using Microsoft.Extensions.DependencyInjection;
using Grillbot.Models.Channelboard;
using Grillbot.Database.Entity.Users;

namespace Grillbot.Services.Channelboard
{
    public class ChannelStats
    {
        public const int ChannelboardTakeTop = 10;

        private IServiceProvider Provider { get; }

        private DiscordSocketClient Discord { get; }

        public ChannelStats(IServiceProvider provider, DiscordSocketClient discord)
        {
            Provider = provider;
            Discord = discord;
        }

        public async Task<Tuple<int, long, DateTime>> GetValueAsync(SocketGuild guild, ulong channelID, SocketUser socketUser)
        {
            if (!(await CanUserToChannelAsync(guild, channelID, socketUser)))
                return null;

            var channels = GetAllChannels(guild);
            var channel = channels.FirstOrDefault(o => o.ChannelIDSnowflake == channelID);

            if (channel == null)
                return new Tuple<int, long, DateTime>(default, default, default);

            var position = channels.FindIndex(o => o.ChannelIDSnowflake == channelID) + 1;
            return new Tuple<int, long, DateTime>(position, channel.Count, channel.LastMessageAt);
        }

        public async Task<List<ChannelStatItem>> GetChannelboardDataAsync(SocketGuild guild, SocketUser user, int? limit = null, bool full = false)
        {
            var result = new List<ChannelStatItem>();

            await guild.SyncGuildAsync();
            foreach (var channel in GetAllChannels(guild))
            {
                if (!full && !(await CanUserToChannelAsync(guild, channel.ChannelIDSnowflake, user)))
                    continue;

                var textChannel = guild.GetTextChannel(channel.ChannelIDSnowflake);
                result.Add(new ChannelStatItem(textChannel, channel));
            }

            var resultData = result
                .OrderByDescending(o => o.Count)
                .ThenByDescending(o => o.LastMessageAt)
                .AsEnumerable();

            if (limit != null)
                resultData = result.Take(limit.Value);

            return resultData.ToList();
        }

        public async Task<List<ChannelStatItem>> GetFullChannelboardAsync()
        {
            var channels = new List<ChannelStatItem>();

            foreach (var guild in Discord.Guilds)
            {
                var result = await GetChannelboardDataAsync(guild, null, null, true);

                if (result?.Count > 0)
                    channels.AddRange(result);
            }

            return channels
                .Where(o => o.Channel != null)
                .OrderByDescending(o => o.Count)
                .ThenByDescending(o => o.LastMessageAt)
                .ThenBy(o => o.Channel.Id)
                .ToList();
        }

        private async Task<bool> CanUserToChannelAsync(SocketGuild guild, ulong channelID, IUser user)
        {
            var channel = guild.GetChannel(channelID);
            if (channel == null)
                return false;

            return channel.Users.Any(o => o.Id == user.Id);
        }

        public async Task<List<string>> CleanOldChannels(SocketGuild guild)
        {
            await guild.SyncGuildAsync();

            var removed = new HashSet<string>();

            using var scope = Provider.CreateScope();
            using var repository = scope.ServiceProvider.GetService<ChannelStatsRepository>();
            var channels = repository.GetAllChannels(guild.Id);

            foreach (var channelID in channels)
            {
                var channelIdSnowflake = Convert.ToUInt64(channelID);
                var discordChannel = guild.GetChannel(channelIdSnowflake);

                if (discordChannel == null)
                {
                    removed.Add($"Kanál {channelID} byl smazán.");
                    repository.RemoveChannel(channelIdSnowflake);
                }
            }

            return removed.Count == 0 ? null : removed.ToList();
        }

        /// <summary>
        /// Get all channels for specific guild.
        /// </summary>
        private List<UserChannel> GetAllChannels(SocketGuild guild)
        {
            using var scope = Provider.CreateScope();
            using var repository = scope.ServiceProvider.GetService<ChannelStatsRepository>();
            return repository.GetGroupedStats(guild.Id);
        }
    }
}