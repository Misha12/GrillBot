using Discord.WebSocket;
using Grillbot.Extensions.Discord;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Services.TempUnverify
{
    public class TempUnverifyHelper
    {
        public async Task FindAndToggleMutedRoleAsync(SocketGuildUser user, SocketGuild guild, bool set)
        {
            await guild.SyncGuildAsync();

            var mutedRole = guild.Roles
                .FirstOrDefault(o => string.Equals(o.Name, "muted", StringComparison.InvariantCultureIgnoreCase));

            if (mutedRole == null)
                return; // Mute role not exists on this server.

            if (set)
            {
                if (user.Roles.Any(o => o.Id == mutedRole.Id))
                    return; // User now have muted role.

                await user.AddRoleAsync(mutedRole);
            }
            else
            {
                await user.RemoveRoleAsync(mutedRole);
            }
        }
    }
}
