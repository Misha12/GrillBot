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
using Grillbot.Services.UserManagement;
using Grillbot.Database.Entity.Users;

namespace Grillbot.Services.Channelboard
{
    public class ChannelStats
    {
        public const int ChannelboardTakeTop = 10;

        private IServiceProvider Provider { get; }
        private UserService UserService { get; }

        public ChannelStats(IServiceProvider provider, UserService userService)
        {
            Provider = provider;
            UserService = userService;
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

        public async Task<List<ChannelStatItem>> GetChannelboardDataAsync(SocketGuild guild, SocketUser user, int? limit = null)
        {
            var result = new List<ChannelStatItem>();

            foreach(var channel in GetAllChannels(guild))
            {
                if (!(await CanUserToChannelAsync(guild, channel.ChannelIDSnowflake, user)))
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

        private async Task<bool> CanUserToChannelAsync(SocketGuild guild, ulong channelID, IUser user)
        {
            await guild.SyncGuildAsync();

            var channel = guild.GetChannel(channelID);
            if (channel == null)
                return false;

            return channel.Users.Any(o => o.Id == user.Id);
        }

        public async Task<List<string>> CleanOldChannels(SocketGuild guild)
        {
            await guild.SyncGuildAsync();

            var removed = new HashSet<string>();

            using var repository = Provider.GetService<ChannelStatsRepository>();
            foreach(var user in UserService.Users)
            {
                foreach(var channel in user.Value.Channels.ToList())
                {
                    var discordChannel = guild.GetChannel(channel.ChannelIDSnowflake);

                    if(discordChannel == null)
                    {
                        removed.Add($"Kanál {channel.ChannelIDSnowflake} byl smazán.");
                        repository.RemoveChannel(channel.ChannelIDSnowflake);
                        UserService.RemoveChannel(user.Key, channel.ChannelIDSnowflake);
                    }
                }
            }

            if (removed.Count == 0)
                return null;

            return removed.ToList();
        }

        private List<UserChannel> GetAllChannels(SocketGuild guild)
        {
            return UserService.Users.Values
                .Where(o => o.GuildIDSnowflake == guild.Id)
                .SelectMany(o => o.Channels)
                .GroupBy(o => o.ChannelIDSnowflake)
                .Select(o => new UserChannel()
                {
                    ChannelIDSnowflake = o.Key,
                    Count = o.Sum(x => x.Count),
                    LastMessageAt = o.Max(x => x.LastMessageAt)
                })
                .OrderByDescending(o => o.Count)
                .ThenByDescending(o => o.LastMessageAt)
                .ToList();
        }
    }
}