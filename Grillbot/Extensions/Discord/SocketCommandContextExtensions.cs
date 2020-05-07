using Discord.Commands;
using Discord.WebSocket;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Extensions.Discord
{
    public static class SocketCommandContextExtensions
    {
        public static async Task<SocketGuildUser> ParseGuildUserAsync(this SocketCommandContext context, string externalIdentification = null)
        {
            if (context.Message.MentionedUsers.Count > 0)
                return context.Message.MentionedUsers.OfType<SocketGuildUser>().FirstOrDefault();

            if (string.IsNullOrEmpty(externalIdentification))
                return null;

            return await context.Guild.GetUserFromGuildAsync(externalIdentification);
        }
    }
}
