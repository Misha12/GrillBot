using Grillbot.Database.Entity.Config;
using Grillbot.Enums;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Grillbot.Database.Repository
{
    public class GlobalConfigRepository : RepositoryBase
    {
        public GlobalConfigRepository(GrillBotContext context) : base(context)
        {
        }

        public async Task<string> GetItemAsync(GlobalConfigItems itemKey)
        {
            var key = itemKey.ToString();
            var result = await Context.GlobalConfig
                .SingleOrDefaultAsync(o => o.Key == key);

            return result?.Value;
        }

        public async Task UpdateItemAsync(GlobalConfigItems item, string value)
        {
            var key = item.ToString();

            var result = await Context.GlobalConfig
                .SingleOrDefaultAsync(o => o.Key == key);

            if (result == null)
            {
                result = new GlobalConfigItem()
                {
                    Key = key,
                    Value = value
                };

                Context.GlobalConfig.Add(result);
            }
            else
            {
                result.Value = value;
            }

            await Context.SaveChangesAsync();
        }
    }
}
