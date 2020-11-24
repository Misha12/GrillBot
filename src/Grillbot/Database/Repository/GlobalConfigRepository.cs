using Grillbot.Database.Entity.Config;
using Grillbot.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Database.Repository
{
    public class GlobalConfigRepository : RepositoryBase
    {
        public GlobalConfigRepository(GrillBotContext context) : base(context)
        {
        }

        public IQueryable<Tuple<string, string>> GetAllItems()
        {
            return Context.GlobalConfig.AsQueryable()
                .Select(o => Tuple.Create(o.Key, o.Value));
        }

        public Task<GlobalConfigItem> GetItemAsync(GlobalConfigItems key)
        {
            return Context.GlobalConfig.AsQueryable()
                .SingleOrDefaultAsync(o => o.Key == key.ToString());
        }
    }
}
