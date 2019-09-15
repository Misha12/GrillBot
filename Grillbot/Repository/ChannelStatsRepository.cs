using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grillbot.Repository.Entity;
using Grillbot.Services.Config.Models;
using Microsoft.EntityFrameworkCore;

namespace Grillbot.Repository
{
    public class ChannelStatsRepository : RepositoryBase
    {
        public ChannelStatsRepository(Configuration config) : base(config)
        {
        }

        public async Task<List<ChannelStat>> GetChannelStatistics()
        {
            return await Context.ChannelStats.ToListAsync();
        }

        public async Task UpdateChannelboardStatisticsAsync(Dictionary<ulong, long> dataForUpdate, Dictionary<ulong, DateTime> lastMessageData)
        {
            foreach(var itemForUpdate in dataForUpdate)
            {
                var entity = await Context.ChannelStats.FirstOrDefaultAsync(o => o.ID == itemForUpdate.Key.ToString());
                if (entity == null)
                {
                    entity = new ChannelStat()
                    {
                        Count = itemForUpdate.Value,
                        SnowflakeID = itemForUpdate.Key,
                        LastMessageAt = lastMessageData[itemForUpdate.Key]
                    };

                    await Context.AddAsync(entity);
                }
                else
                {
                    entity.Count = itemForUpdate.Value;
                    entity.LastMessageAt = lastMessageData[itemForUpdate.Key];
                }
            }

            await Context.SaveChangesAsync();
        }
    }
}
