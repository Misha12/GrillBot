using Discord.WebSocket;
using System;

namespace Grillbot.Extensions.Discord
{
    public static class SocketRoleExtensions
    {
        public static bool IsMutedRole(this SocketRole role)
        {
            return role.Name.Equals("muted", StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
