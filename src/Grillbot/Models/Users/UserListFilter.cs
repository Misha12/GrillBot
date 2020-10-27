using Discord.WebSocket;
using Grillbot.Enums;
using System.Collections.Generic;

namespace Grillbot.Models.Users
{
    public class UserListFilter
    {
        public SocketGuild Guild { get; set; }
        public List<SocketGuildUser> UserIDs { get; set; }
        public WebAdminUserOrder Order { get; set; }
        public string InviteCode { get; set; }
        public bool OnlyWebAdmin { get; set; }
        public bool OnlyApiAccess { get; set; }
        public bool OnlyBotAdmin { get; set; }
        public bool Desc { get; set; }
    }
}
