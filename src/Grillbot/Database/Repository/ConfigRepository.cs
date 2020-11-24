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

        public Task<MethodsConfig> FindConfigAsync(ulong guildID, string group, string command, bool withPerms = true)
        {
            CorrectValue(ref group);
            CorrectValue(ref command);

            return GetBaseQuery(withPerms)
                .SingleOrDefaultAsync(o => o.GuildID == guildID.ToString() && o.Group == group && o.Command == command);
        }

        public IQueryable<MethodsConfig> GetAllMethods(ulong guildID, bool withPermissions = false)
        {
            var query = GetBaseQuery(withPermissions);
            return query.Where(o => o.GuildID == guildID.ToString());
        }

        public void CorrectValue(ref string value)
        {
            if (value == null)
                value = "";
        }

        public Task<bool> ConfigExistsAsync(ulong guildID, string group, string command)
        {
            CorrectValue(ref group);
            CorrectValue(ref command);

            return GetBaseQuery(false)
                .AnyAsync(o => o.GuildID == guildID.ToString() && o.Group == group && o.Command == command);
        }
    }
}
