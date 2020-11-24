using Grillbot.Database.Entity.Users;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Database.Repository
{
    public class ChannelStatsRepository : RepositoryBase
    {
        public ChannelStatsRepository(GrillBotContext context) : base(context)
        {
        }

        public async Task RemoveChannelAsync(ulong channelID)
        {
            var channels = await Context.UserChannels.AsQueryable()
                .Where(o => o.ChannelID == channelID.ToString())
                .ToListAsync();

            Context.UserChannels.RemoveRange(channels);
        }

        public IEnumerable<ulong> GetAllChannels(ulong guildID, List<ulong> currentChannels)
        {
            var guildUsers = Context.Users.AsQueryable()
                .Where(o => o.GuildID == guildID.ToString())
                .Select(o => o.ID);

            var query = Context.UserChannels.AsQueryable()
                .Where(o => guildUsers.Contains(o.UserID))
                .Select(o => o.ChannelID)
                .Distinct();

            return query
                .AsEnumerable()
                .Select(o => Convert.ToUInt64(o))
                .Where(o => !currentChannels.Contains(o));
        }

        public IQueryable<UserChannel> GetGroupedStats(ulong guildID)
        {
            var guild = guildID.ToString();

            var guildUsers = Context.Users.AsQueryable()
                .Where(o => o.GuildID == guild)
                .Select(o => o.ID);

            var query = Context.UserChannels.AsQueryable()
                .Where(o => guildUsers.Contains(o.UserID))
                .GroupBy(o => o.ChannelID)
                .Select(o => new UserChannel()
                {
                    ChannelID = o.Key,
                    Count = o.Sum(x => x.Count),
                    LastMessageAt = o.Max(x => x.LastMessageAt)
                })
                .Where(o => o.Count > 0)
                .OrderByDescending(o => o.Count)
                .ThenByDescending(o => o.LastMessageAt);

            return query;
        }

        public IQueryable<UserChannel> GetChannelsOfUser(long id)
        {
            return Context.UserChannels.AsQueryable()
                .Where(o => o.UserID == id);
        }
    }
}
