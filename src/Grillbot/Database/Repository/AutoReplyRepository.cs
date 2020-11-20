using Grillbot.Database.Entity;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Database.Repository
{
    public class AutoReplyRepository : RepositoryBase
    {
        public AutoReplyRepository(GrillBotContext context) : base(context)
        {
        }

        public IQueryable<AutoReplyItem> GetItems()
        {
            return Context.AutoReply.AsQueryable();
        }

        public Task<AutoReplyItem> FindItemByIdAsync(int id)
        {
            return Context.AutoReply.AsQueryable()
                .SingleOrDefaultAsync(o => o.ID == id);
        }
    }
}
