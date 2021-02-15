using Discord.WebSocket;
using Grillbot.Enums;
using System.Collections.Generic;

namespace Grillbot.Models.Auth
{
    public class AuthViewModel
    {
        public WebAdminLoginResult? LoginResult { get; set; }
        public List<SocketGuild> Guilds { get; }

        public AuthViewModel(List<SocketGuild> guilds, WebAdminLoginResult? loginResult = null)
        {
            LoginResult = loginResult;
            Guilds = guilds;
        }
    }
}
