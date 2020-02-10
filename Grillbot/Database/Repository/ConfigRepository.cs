using Grillbot.Database.Entity.MethodConfig;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Database.Repository
{
    public class ConfigRepository : RepositoryBase
    {
        public ConfigRepository(GrillBotContext context) : base(context)
        {
        }

        private IQueryable<MethodsConfig> GetBaseQuery(bool includePermissions)
        {
            var query = Context.MethodsConfig.AsQueryable();

            if (includePermissions)
                query = query.Include(o => o.Permissions);

            return query;
        }

        public MethodsConfig FindConfig(ulong guildID, string group, string command)
        {
            var query = GetBaseQuery(true);
            return query.FirstOrDefault(o => o.GuildID == guildID.ToString() && o.Group == group && o.Command == command);
        }


    }
}
