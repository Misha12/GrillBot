using Discord;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;

namespace Grillbot.Extensions.Discord
{
    public static class RoleColectionExtensions
    {
        public static SocketRole FindHighestRoleWithColor(this IReadOnlyCollection<SocketRole> roles)
        {
            return roles.Where(o => o.Color != Color.Default).OrderByDescending(o => o.Position).FirstOrDefault();
        }
    }
}
