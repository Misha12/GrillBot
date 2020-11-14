using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grillbot.Database.Entity;
using Microsoft.EntityFrameworkCore;

namespace Grillbot.Database.Repository
{
    public class TeamSearchRepository : RepositoryBase
    {
        public TeamSearchRepository(GrillBotContext context) : base(context)
        {
        }

        public List<TeamSearch> GetSearches(int[] ids)
        {
            var query = Queryable.Where(Context.TeamSearch, o => ids.Contains(o.Id));
            return query.ToList();
        }

        public List<TeamSearch> GetAllSearches(string channelID)
        {
            var query = Context.TeamSearch.AsQueryable();

            if (!string.IsNullOrEmpty(channelID))
                query = query.Where(o => o.ChannelId == channelID);

            return query.ToList();
        }

        public async Task AddSearchAsync(ulong guildID, ulong userID, ulong channelID, ulong messageID)
        {
            var entity = new TeamSearch()
            {
                ChannelIDSnowflake = channelID,
                GuildIDSnowflake = guildID,
                MessageIDSnowflake = messageID,
                UserIDSnowflake = userID
            };

            await Context.TeamSearch.AddAsync(entity);
            await Context.SaveChangesAsync();
        }

        public async Task RemoveSearchAsync(int id)
        {
            var entity = await FindSearchByIDAsync(id);
            await RemoveSearchAsync(entity);
        }

        public Task RemoveSearchAsync(TeamSearch entity)
        {
            if (entity == null) return Task.CompletedTask;

            Context.TeamSearch.Remove(entity);
            return Context.SaveChangesAsync();
        }

        public Task<TeamSearch> FindSearchByIDAsync(int id)
        {
            return Context.TeamSearch.AsQueryable()
                .FirstOrDefaultAsync(o => o.Id == id);
        }
    }
}