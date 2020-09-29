using Discord;
using Discord.WebSocket;

namespace Grillbot.Extensions.Discord
{
    public static class SocketGuildChannelExtensions
    {
        public static bool HaveAccess(this SocketGuildChannel channel, SocketGuildUser user)
        {
            if (channel.GetUser(user.Id) != null)
                return true;

            if (channel.PermissionOverwrites.Count == 0)
                return true; // Default permissions. Access all

            var overwrite = channel.GetPermissionOverwrite(user);
            var everyonePerm = channel.GetPermissionOverwrite(user.Guild.EveryoneRole);

            if (overwrite != null && overwrite.Value.ViewChannel == PermValue.Allow)
                return true;

            foreach (var role in user.Roles)
            {
                var roleOverwrite = channel.GetPermissionOverwrite(role);
                if (roleOverwrite == null) continue;

                if (roleOverwrite.Value.ViewChannel == PermValue.Allow)
                    return true;
            }

            return everyonePerm != null && (everyonePerm.Value.ViewChannel == PermValue.Allow || everyonePerm.Value.ViewChannel == PermValue.Inherit);
        }
    }
}
