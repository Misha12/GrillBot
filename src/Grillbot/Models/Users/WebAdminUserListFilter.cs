using Discord.WebSocket;
using Grillbot.Enums;
using System.Collections.Generic;

namespace Grillbot.Models.Users
{
    public class WebAdminUserListFilter
    {
        public ulong GuildID { get; set; }
        public string UserQuery { get; set; }
        public WebAdminUserOrder Order { get; set; }
        public bool SortDesc { get; set; } = true;
        public int Page { get; set; } = 1;
        public string UsedInviteCode { get; set; }

        public bool WebAdmin { get; set; }
        public bool ApiAccess { get; set; }
        public bool BotAdmin { get; set; }

        public UserListFilter CreateQueryFilter(SocketGuild guild, List<SocketGuildUser> users)
        {
            return new UserListFilter()
            {
                Desc = SortDesc,
                Guild = guild,
                InviteCode = UsedInviteCode,
                OnlyApiAccess = ApiAccess,
                OnlyBotAdmin = BotAdmin,
                OnlyWebAdmin = WebAdmin,
                Order = Order,
                Users = users
            };
        }
    }
}
