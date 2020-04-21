using Discord.WebSocket;
using System.Linq;

namespace Grillbot.Extensions.Discord
{
    public static class SocketGuildUserExtensions
    {
        public static SocketRole FindHighestRole(this SocketGuildUser user)
        {
            return user.Roles.OrderByDescending(o => o.Position).FirstOrDefault();
        }
    }
}
