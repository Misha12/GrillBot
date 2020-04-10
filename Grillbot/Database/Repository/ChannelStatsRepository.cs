using System.Collections.Generic;
using System.Linq;
using Discord.WebSocket;
using Grillbot.Database.Entity;

namespace Grillbot.Database.Repository
{
    public class ChannelStatsRepository : RepositoryBase
    {
        public ChannelStatsRepository(GrillBotContext context) : base(context)
        {
        }

        public List<ChannelStat> GetChannelStatistics(SocketGuild guild)
        {
            var query = Context.ChannelStats.AsQueryable();

            if (guild == null)
                return query.ToList();

            var guildID = guild.Id.ToString();
            return query
                .Where(o => o.GuildID == guildID)
                .ToList();
        }

        public void UpdateChannelboard(List<ChannelStat> updatedItems)
        {
            foreach(var item in updatedItems)
            {
                var entity = Context.ChannelStats.FirstOrDefault(o => o.GuildID == item.GuildID && o.ID == item.ID);

                if(entity == null)
                {
                    entity = new ChannelStat()
                    {
                        Count = item.Count,
                        ID = item.ID,
                        LastMessageAt = item.LastMessageAt,
                        GuildID = item.GuildID
                    };

                    Context.Set<ChannelStat>().Add(entity);
                }
                else
                {
                    entity.Count = item.Count;
                    entity.LastMessageAt = item.LastMessageAt;
                }
            }

            Context.SaveChanges();
        }

        public void RemoveChannel(ChannelStat channelStat)
        {
            var entity = Context.ChannelStats.FirstOrDefault(o => o.GuildID == channelStat.GuildID && o.ID == channelStat.ID);

            if (entity == null)
                return;

            Context.Remove(entity);
            Context.SaveChanges();
        }
    }
}
