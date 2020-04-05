using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grillbot.Database.Entity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Schema;

namespace Grillbot.Database.Repository
{
    public class TeamSearchRepository : RepositoryBase
    {
        public TeamSearchRepository(GrillBotContext context) : base(context)
        {
        }

        public List<TeamSearch> GetAllSearches(string channelID)
        {
            var query = Context.TeamSearch.AsQueryable();

            if (!string.IsNullOrEmpty(channelID))
                query = query.Where(o => o.ChannelId == channelID);

            return query.ToList();
        }

        public void AddSearch(ulong guildID, ulong userID, ulong channelID, ulong messageID)
        {
            var entity = new TeamSearch()
            {
                ChannelIDSnowflake = channelID,
                GuildIDSnowflake = guildID,
                MessageIDSnowflake = messageID,
                UserIDSnowflake = userID
            };

            Context.TeamSearch.Add(entity);
            Context.SaveChanges();
        }

        public async Task RemoveSearchAsync(int id)
        {
            var row = await FindSearchByIDAsync(id).ConfigureAwait(false);

            if (row == null)
                return;

            Context.TeamSearch.Remove(row);
            await Context.SaveChangesAsync().ConfigureAwait(false);
        }

        public void RemoveSearch(int id)
        {
            var entity = FindSearchByID(id);
            if (entity == null) return;

            Context.TeamSearch.Remove(entity);
            Context.SaveChanges();
        }

        public async Task<TeamSearch> FindSearchByIDAsync(int id)
        {
            return await Context.TeamSearch.FirstOrDefaultAsync(o => o.Id == id).ConfigureAwait(false);
        }

        public TeamSearch FindSearchByID(int id)
        {
            return Context.TeamSearch.FirstOrDefault(o => o.Id == id);
        }
    }
}