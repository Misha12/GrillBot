using Grillbot.Repository.Entity;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace Grillbot.Repository
{
    public class AutoReplyRepository : RepositoryBase
    {
        public AutoReplyRepository(IConfiguration config) : base(config)
        {
        }

        public List<AutoReplyItem> GetAllItems()
        {
            return Context.AutoReply.ToList();
        }
    }
}
