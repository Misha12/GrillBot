using Grillbot.Repository.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grillbot.Repository
{
    public class AutoReplyRepository : RepositoryBase
    {
        public AutoReplyRepository(IConfiguration config) : base(config)
        {
        }

        public async Task<List<AutoReplyItem>> GetAllItemsAsync()
        {
            return await Context.AutoReply.ToListAsync();
        }
    }
}
