using Discord.WebSocket;
using Grillbot.Models.Unverify;
using System.Threading.Tasks;

namespace Grillbot.Services.Unverify.WebAdmin
{
    public class UnverifyModelConverter
    {
        private DiscordSocketClient DiscordClient { get; }
        private SearchService UserSearch { get; }

        private const int PageSize = 10;

        public UnverifyModelConverter(DiscordSocketClient discord, SearchService searchService)
        {
            DiscordClient = discord;
            UserSearch = searchService;
        }

        public async Task<UnverifyAuditFilter> ConvertAuditFilter(UnverifyAuditFilterFormData formData)
        {
            if (formData.Page < 0)
                formData.Page = 0;

            var result = new UnverifyAuditFilter()
            {
                DateTimeFrom = formData.DateTimeFrom,
                DateTimeTo = formData.DateTimeTo,
                Guild = DiscordClient.GetGuild(formData.GuildID),
                Operation = formData.Operation,
                OrderAsc = formData.OrderAsc,
                Skip = (formData.Page == 0 ? 0 : formData.Page - 1) * PageSize,
                Take = PageSize
            };

            result.FromUsers = await UserSearch.FindUsersAsync(result.Guild, formData.FromUserQuery);
            result.ToUsers = await UserSearch.FindUsersAsync(result.Guild, formData.ToUserQuery);

            return result;
        }
    }
}
