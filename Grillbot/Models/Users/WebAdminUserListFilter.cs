using Grillbot.Enums;

namespace Grillbot.Models.Users
{
    public class WebAdminUserListFilter
    {
        public ulong GuildID { get; set; }
        public string UserQuery { get; set; }
        public WebAdminUserOrder Order { get; set; } = WebAdminUserOrder.Points;
        public bool SortDesc { get; set; } = true;
        public int Page { get; set; } = 1;
        public string UsedInviteCode { get; set; }
    }
}
