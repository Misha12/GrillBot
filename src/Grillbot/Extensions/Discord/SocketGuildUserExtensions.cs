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

        public static bool IsGuildOwner(this SocketGuildUser user)
        {
            return IsGuildOwner(user, user.Guild);
        }

        public static bool IsGuildOwner(this SocketGuildUser user, SocketGuild guild)
        {
            return guild.OwnerId == user.Id;
        }

        public static bool IsMuted(this SocketGuildUser user)
        {
            return user.IsMuted || user.IsDeafened;
        }

        public static bool IsSelfMuted(this SocketGuildUser user)
        {
            return user.IsSelfMuted || user.IsSelfDeafened;
        }
    }
}
