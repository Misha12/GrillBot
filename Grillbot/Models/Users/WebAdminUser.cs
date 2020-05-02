using Discord.WebSocket;

namespace Grillbot.Models.Users
{
    public class WebAdminUser
    {
        public SocketGuild Guild { get; set; }
        public SocketGuildUser User { get; set; }

        public WebAdminUser(SocketGuild guild, SocketGuildUser user)
        {
            Guild = guild;
            User = user;
        }
    }
}
