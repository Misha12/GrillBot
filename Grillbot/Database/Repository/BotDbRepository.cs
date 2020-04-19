using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Grillbot.Database.Repository
{
    public class BotDbRepository : RepositoryBase
    {
        public BotDbRepository(GrillBotContext context) : base(context)
        {
        }

        public async Task<Dictionary<string, int>> GetTableRowsCount()
        {
            var autoReplyCount = await GetRowCountAsync(Context.AutoReply);
            var emoteStatsCount = await GetRowCountAsync(Context.EmoteStats);
            var channelStatsCount = await GetRowCountAsync(Context.ChannelStats);
            var teamSearchCount = await GetRowCountAsync(Context.TeamSearch);
            var tempUnverifyCount = await GetRowCountAsync(Context.TempUnverify);
            var unverifyLogCount = await GetRowCountAsync(Context.UnverifyLog);
            var birthdayCount = await GetRowCountAsync(Context.Birthdays);
            var methodsConfigCount = await GetRowCountAsync(Context.MethodsConfig);
            var methodPermsCount = await GetRowCountAsync(Context.MethodPerms);
            var webAuthPermCount = await GetRowCountAsync(Context.WebAdminPerms);

            var counters = new Dictionary<string, int>()
            {
                { "AutoReply", autoReplyCount },
                { "EmoteStats", emoteStatsCount },
                { "ChannelStats", channelStatsCount },
                { "TeamSearch", teamSearchCount },
                { "TempUnverify", tempUnverifyCount },
                { "UnverifyLog", unverifyLogCount },
                { "Birthdays", birthdayCount },
                { "MethodsConfig", methodsConfigCount },
                { "MethodPerms", methodPermsCount },
                { "WebAuthPerm", webAuthPermCount }
            };

            return counters
                .OrderBy(o => o.Key)
                .ThenBy(o => o.Value)
                .ToDictionary(o => o.Key, o => o.Value);
        }

        private async Task<int> GetRowCountAsync<T>(IQueryable<T> table) => await table.CountAsync();
    }
}
