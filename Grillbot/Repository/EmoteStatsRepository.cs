using System.Collections.Generic;
using System.Threading.Tasks;
using Grillbot.Repository.Entity;
using Grillbot.Services.Config.Models;
using Microsoft.EntityFrameworkCore;

namespace Grillbot.Repository
{
    public class EmoteStatsRepository : RepositoryBase
    {
        public EmoteStatsRepository(Configuration config) : base(config)
        {
        }

        public async Task<List<EmoteStat>> GetEmoteStatistics()
        {
            return await Context.EmoteStats.ToListAsync().ConfigureAwait(false);
        }

        public async Task UpdateEmoteStatistics(Dictionary<string, EmoteStat> changedData)
        {
            foreach(var changedItem in changedData.Values)
            {
                var entity = await Context.EmoteStats.FirstOrDefaultAsync(o => o.EmoteID == changedItem.EmoteID).ConfigureAwait(false);

                if(entity == null)
                {
                    await Context.EmoteStats.AddAsync(changedItem).ConfigureAwait(false);
                }
                else
                {
                    entity.Count = changedItem.Count;
                    entity.LastOccuredAt = changedItem.LastOccuredAt;
                }
            }

            await Context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task RemoveEmoteAsync(string emoteId)
        {
            var entity = await Context.EmoteStats.FirstOrDefaultAsync(o => o.EmoteID == emoteId).ConfigureAwait(false);

            if (entity == null)
                return;

            Context.Remove(entity);
            await Context.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
