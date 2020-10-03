using Discord.WebSocket;
using System.Collections.Generic;

namespace Grillbot.Models.Users
{
    public class WebAdminUserListViewModel
    {
        public List<DiscordUser> Users { get; set; }
        public List<SocketGuild> Guilds { get; set; }
        public WebAdminUserListFilter Filter { get; set; }
        
        public WebAdminUserListViewModel(List<DiscordUser> users, List<SocketGuild> guilds, WebAdminUserListFilter filter)
        {
            Users = users;
            Guilds = guilds;
            Filter = filter;
        }
    }
}
