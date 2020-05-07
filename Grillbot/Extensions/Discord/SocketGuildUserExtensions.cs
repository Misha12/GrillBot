using Discord;
using Discord.WebSocket;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Extensions.Discord
{
    public static class SocketGuildUserExtensions
    {
        public static SocketRole FindHighestRole(this SocketGuildUser user)
        {
            return user.Roles.OrderByDescending(o => o.Position).FirstOrDefault();
        }

        public static async Task SetRoleAsync(this SocketGuildUser user, IRole role)
        {
            var haveRole = user.Roles.Any(o => o.Id == role.Id);

            if (!haveRole)
                await user.AddRoleAsync(role);
        }
    }
}
