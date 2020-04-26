using Discord.WebSocket;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Grillbot.Extensions.Discord
{
    public static class DiscordSocketClientExtensions
    {
        public static async Task<SocketGuildUser> GetUserFromClaimsAsync(this DiscordSocketClient client, ClaimsPrincipal user)
        {
            var userID = Convert.ToUInt64(user.FindFirstValue(ClaimTypes.NameIdentifier));
            var guildID = Convert.ToUInt64(user.FindFirstValue(ClaimTypes.UserData));
            var guild = client.GetGuild(guildID);

            return await guild.GetUserFromGuildAsync(userID);
        }
    }
}
