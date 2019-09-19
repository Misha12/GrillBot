using System.Linq;
using System.Threading.Tasks;
using Grillbot.Repository.Entity;
using Grillbot.Services.Config.Models;
using Microsoft.EntityFrameworkCore;

namespace Grillbot.Repository
{
    public class TeamSearchRepository : RepositoryBase
    {
        public TeamSearchRepository(Configuration config) : base(config)
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

        public async Task RemoveSearchAsync(int id)
        {
            var row = await FindSearchByID(id);

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