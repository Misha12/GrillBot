using Grillbot.Database.Entity.Users;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grillbot.Database.Repository
{
    public class ChannelStatsRepository : RepositoryBase
    {
        public ChannelStatsRepository(GrillBotContext context) : base(context)
        {
        }

        public void RemoveChannel(ulong channelID)
        {
            var channels = Context.UserChannels.AsQueryable()
                .Where(o => o.ChannelID == channelID.ToString())
                .ToList();

            Context.UserChannels.RemoveRange(channels);
        }

        public List<ulong> GetAllChannels(ulong guildID, List<ulong> currentChannels)
        {
            var channelsInGuild = currentChannels.Select(o => o.ToString()).ToList();

            var userIdsInGuild = Context.Users.AsQueryable()
                .Where(o => o.GuildID == guildID.ToString())
                .Select(o => o.ID).Distinct().ToList();

            return Context.UserChannels.AsQueryable()
                .Where(o => userIdsInGuild.Contains(o.UserID) && !channelsInGuild.Contains(o.ChannelID))
                .Select(o => o.ChannelID).Distinct()
                .AsEnumerable().Select(o => Convert.ToUInt64(o)).ToList();
        }

        public List<UserChannel> GetGroupedStats(ulong guildID)
        {
            var guild = guildID.ToString();

            var userIDs = Context.Users.AsQueryable()
                .Where(o => o.GuildID == guild && o.Points > 0)
                .Select(o => o.ID)
                .Distinct()
                .ToList();

            return Context.UserChannels.AsQueryable()
                .Where(o => userIDs.Contains(o.UserID))
                .GroupBy(o => o.ChannelID)
                .Select(o => new UserChannel()
                {
                    ChannelID = o.Key,
                    Count = o.Sum(x => x.Count),
                    LastMessageAt = o.Max(x => x.LastMessageAt)
                })
                .OrderByDescending(o => o.Count)
                .ThenByDescending(o => o.LastMessageAt)
                .ToList();
        }

        public UserChannel GetGroupedChannel(ulong guildID, ulong channelID)
        {
            var guild = guildID.ToString();
            var channel = channelID.ToString();

            var userIDs = Context.Users.AsQueryable()
                .Where(o => o.GuildID == guild && o.Points > 0)
                .Select(o => o.ID)
                .Distinct()
                .ToList();

            return Context.UserChannels.AsQueryable()
                .Where(o => o.ChannelID == channel && userIDs.Contains(o.ID))
                .GroupBy(o => o.ChannelID)
                .Select(o => new UserChannel()
                {
                    ChannelID = o.Key,
                    Count = o.Sum(x => x.Count),
                    LastMessageAt = o.Max(x => x.LastMessageAt)
                })
                .FirstOrDefault();
        }

        public IQueryable<UserChannel> GetChannelsOfUser(long id)
        {
            return Context.UserChannels.AsQueryable()
                .Where(o => o.UserID == id);
        }
    }
}
