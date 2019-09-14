using System.Threading.Tasks;
using Discord.WebSocket;
using Grillbot.Extensions;
using Grillbot.Services.Logger.LoggerMethods.LogEmbed;
using Microsoft.Extensions.Configuration;

namespace Grillbot.Services.Logger.LoggerMethods
{
    public class UserJoined : LoggerMethodBase
    {
        public UserJoined(DiscordSocketClient client, IConfiguration config) : base(client, config, null)
        {
        }

        public async Task ProcessAsync(SocketGuildUser user)
        {
            var logEmbedBuilder = new LogEmbedBuilder("Připojil se uživatel", LogEmbedType.UserJoined);

            var createdAt = user.CreatedAt.LocalDateTime.ToLocaleDatetime();

            logEmbedBuilder
                .SetAuthor(user)
                .SetTimestamp(true)
                .SetFooter($"MemberID: {user.Id}")
                .AddField("Založen", createdAt)
                .AddField("Počet členů na serveru", user.Guild.MemberCount);

            var loggerRoom = GetLoggerRoom();
            await loggerRoom.SendMessageAsync(embed: logEmbedBuilder.Build());
        }
    }
}
