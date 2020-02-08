using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grillbot.Database.Entity;
using Microsoft.EntityFrameworkCore;

namespace Grillbot.Database.Repository
{
    public class ChannelStatsRepository : RepositoryBase
    {
        public ChannelStatsRepository(GrillBotContext context) : base(context)
        {
        }

        public async Task<List<ChannelStat>> GetChannelStatistics()
        {
            return await Context.ChannelStats.ToListAsync().ConfigureAwait(false);
        }

        public async Task UpdateChannelboardStatisticsAsync(Dictionary<ulong, long> dataForUpdate, Dictionary<ulong, DateTime> lastMessageData)
        {
            foreach(var itemForUpdate in dataForUpdate)
            {
                var entity = await Context.ChannelStats.FirstOrDefaultAsync(o => o.ID == itemForUpdate.Key.ToString()).ConfigureAwait(false);
                if (entity == null)
                {
                    entity = new ChannelStat()
                    {
                        Count = itemForUpdate.Value,
                        SnowflakeID = itemForUpdate.Key,
                        LastMessageAt = lastMessageData[itemForUpdate.Key]
                    };

                    await Context.AddAsync(entity).ConfigureAwait(false);
                }
                else
                {
                    entity.Count = itemForUpdate.Value;
                    entity.LastMessageAt = lastMessageData[itemForUpdate.Key];
                }
            }

            await Context.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
