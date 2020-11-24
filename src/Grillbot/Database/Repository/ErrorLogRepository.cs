using Grillbot.Database.Entity;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Database.Repository
{
    public class ErrorLogRepository : RepositoryBase
    {
        public ErrorLogRepository(GrillBotContext context) : base(context)
        {
        }

        public Task<ErrorLogItem> FindLogByIDAsync(long id)
        {
            return Context.Errors.AsQueryable()
                .SingleOrDefaultAsync(o => o.ID == id);
        }

        public IQueryable<ErrorLogItem> GetLastLogs(int topCount)
        {
            return Context.Errors.AsQueryable()
                .OrderByDescending(o => o.ID)
                .Take(topCount);
        }
    }
}
