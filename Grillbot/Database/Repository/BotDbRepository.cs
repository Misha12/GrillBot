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
            var autoReplyCount = await GetRowCount(Context.AutoReply);
            var emoteStatsCount = await GetRowCount(Context.EmoteStats);
            var channelStatsCount = await GetRowCount(Context.ChannelStats);
            var teamSearchCount = await GetRowCount(Context.TeamSearch);
            var tempUnverifyCount = await GetRowCount(Context.TempUnverify);
            var unverifyLogCount = await GetRowCount(Context.UnverifyLog);
            var commandLogCount = await GetRowCount(Context.CommandLog);
            var birthdayCount = await GetRowCount(Context.Birthdays);
            var methodsConfigCount = await GetRowCount(Context.MethodsConfig);
            var methodPermsCount = await GetRowCount(Context.MethodPerms);

            return new Dictionary<string, int>()
            {
                { "AutoReply", autoReplyCount },
                { "EmoteStats", emoteStatsCount },
                { "ChannelStats", channelStatsCount },
                { "TeamSearch", teamSearchCount },
                { "TempUnverify", tempUnverifyCount },
                { "UnverifyLog", unverifyLogCount },
                { "CommandLog", commandLogCount },
                { "Birthdays", birthdayCount },
                { "MethodsConfig", methodsConfigCount },
                { "MethodPerms", methodPermsCount }
            };
        }

        private async Task<int> GetRowCount<T>(IQueryable<T> table) => await table.CountAsync();
    }
}
