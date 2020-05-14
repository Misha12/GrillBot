using Grillbot.Database.Entity.Users;
using Microsoft.EntityFrameworkCore;
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
            Context.SaveChanges();
        }

        public List<string> GetAllChannels(ulong guildID)
        {
            var guild = guildID.ToString();

            return Context.UserChannels.AsQueryable()
                .Include(o => o.User)
                .Where(o => o.User.GuildID == guild)
                .Select(o => o.ChannelID)
                .Distinct()
                .ToList();
        }

        public List<UserChannel> GetGroupedStats(ulong guildID)
        {
            var guild = guildID.ToString();

            return Context.UserChannels
                .Include(o => o.User)
                .Where(o => o.User.GuildID == guild)
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
    }
}
