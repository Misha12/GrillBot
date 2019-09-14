using System.Linq;
using System.Threading.Tasks;
using Grillbot.Repository.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Grillbot.Repository
{
    public class TeamSearchRepository : RepositoryBase
    {
        public TeamSearchRepository(IConfiguration config) : base(config)
        {
        }

        public IQueryable<TeamSearch> GetAllSearches()
        {
            return Context.TeamSearch.AsQueryable();
        }

        public async Task AddSearchAsync(ulong userID, ulong channelID, ulong messageID)
        {
            var entity = new TeamSearch()
            {
                UserId = userID.ToString(),
                MessageId = messageID.ToString(),
                ChannelId = channelID.ToString()
            };
            
            await Context.TeamSearch.AddAsync(entity);
            await Context.SaveChangesAsync();
        }

        public async Task RemoveSearch(int id)
        {
            var row = await Context.TeamSearch.FirstOrDefaultAsync(d => d.Id == id);

            if (row == null)
                return;

            Context.TeamSearch.Remove(row);
            await Context.SaveChangesAsync();
        }

        public async Task<TeamSearch> FindSearchByID(int id)
        {
            return await Context.TeamSearch.FirstOrDefaultAsync(o => o.Id == id);
        }
    }
}