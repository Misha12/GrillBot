using Discord.WebSocket;
using System.Collections.Generic;

namespace Grillbot.Models.Auth
{
    public class AuthViewModel
    {
        public bool InvalidLogin { get; set; }
        public List<SocketGuild> Guilds { get; }

        public AuthViewModel(List<SocketGuild> guilds, bool invalidLogin = false)
        {
            Guilds = guilds;
            InvalidLogin = invalidLogin;
        }
    }
}
