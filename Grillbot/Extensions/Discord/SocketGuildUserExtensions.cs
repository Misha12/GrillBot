using Discord;
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

        public static SocketRole FindHighestRoleWithColor(this SocketGuildUser user)
        {
            return user.Roles.Where(o => o.Color != Color.Default).OrderByDescending(o => o.Position).FirstOrDefault();
        }
    }
}
