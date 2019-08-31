using System.Threading.Tasks;
using Discord.WebSocket;
using Grillbot.Services.Logger.LoggerMethods.LogEmbed;
using Microsoft.Extensions.Configuration;

namespace Grillbot.Services.Logger.LoggerMethods
{
    public class UserJoined : LoggerMethodBase
    {
        public UserJoined(DiscordSocketClient client, IConfiguration config) : base(client, config, null)
        {
        }

        public async Task Process(SocketUser user)
        {
            var guildUser = (SocketGuildUser)user;
            var logEmbedBuilder = new LogEmbedBuilder("Připojil se uživatel", LogEmbedType.UserJoined);

            var createdAt = guildUser.CreatedAt.LocalDateTime.ToString("dd. MM. yyyy HH:mm:ss");

            logEmbedBuilder
                .SetAuthor(user)
                .SetTimestamp(true)
                .SetFooter($"MemberID: {guildUser.Id}")
                .AddField("Založen", createdAt)
                .AddField("Počet členů na serveru", guildUser.Guild.MemberCount);

            var loggerRoom = GetLoggerRoom();
            await loggerRoom.SendMessageAsync(embed: logEmbedBuilder.Build());
        }
    }
}
