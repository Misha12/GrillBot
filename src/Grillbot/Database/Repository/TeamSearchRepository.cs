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

        public IQueryable<TeamSearch> GetSearches(int[] ids)
        {
            return Context.TeamSearch.AsQueryable()
                .Where(o => ids.Contains(o.Id));
        }

        public IQueryable<TeamSearch> GetAllSearches(string channelId)
        {
            var query = Context.TeamSearch.AsQueryable();

            if (!string.IsNullOrEmpty(channelId))
                query = query.Where(o => o.ChannelId == channelId);

            return query;
        }

        public Task<TeamSearch> FindSearchByIDAsync(int id)
        {
            return Context.TeamSearch.AsQueryable()
                .FirstOrDefaultAsync(o => o.Id == id);
        }
    }
}