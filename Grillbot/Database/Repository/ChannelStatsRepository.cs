using System.Collections.Generic;
using System.Linq;
using Grillbot.Database.Entity;

namespace Grillbot.Database.Repository
{
    public class ChannelStatsRepository : RepositoryBase
    {
        public ChannelStatsRepository(GrillBotContext context) : base(context)
        {
        }

        public List<ChannelStat> GetChannelStatistics() => Context.ChannelStats.ToList();

        public void UpdateChannelboard(List<ChannelStat> updatedItems)
        {
            foreach(var item in updatedItems)
            {
                var entity = Context.ChannelStats.FirstOrDefault(o => o.ID == item.ID);

                if(entity == null)
                {
                    entity = new ChannelStat()
                    {
                        Count = item.Count,
                        ID = item.ID,
                        LastMessageAt = item.LastMessageAt
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
    }
}
