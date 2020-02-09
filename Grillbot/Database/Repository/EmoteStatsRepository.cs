using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grillbot.Database.Entity;
using Microsoft.EntityFrameworkCore;

namespace Grillbot.Database.Repository
{
    public class EmoteStatsRepository : RepositoryBase
    {
        public EmoteStatsRepository(GrillBotContext context) : base(context)
        {
        }

        public List<EmoteStat> GetEmoteStatistics() => Context.EmoteStats.ToList();

        public void UpdateEmoteStatistics(Dictionary<string, EmoteStat> changedData)
        {
            foreach(var changedItem in changedData.Values)
            {
                var entity = Context.EmoteStats.FirstOrDefault(o => o.EmoteID == changedItem.EmoteID);

                if(entity == null)
                {
                    Context.EmoteStats.Add(changedItem);
                }
                else
                {
                    entity.Count = changedItem.Count;
                    entity.LastOccuredAt = changedItem.LastOccuredAt;
                }
            }

            Context.SaveChanges();
        }

        public void RemoveEmote(string emoteId)
        {
            var entity = Context.EmoteStats.FirstOrDefault(o => o.EmoteID == emoteId);

            if (entity == null)
                return;

            Context.Remove(entity);
            Context.SaveChanges();
        }
    }
}
