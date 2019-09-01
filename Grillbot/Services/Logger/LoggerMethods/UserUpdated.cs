using System.Threading.Tasks;
using Discord.WebSocket;
using Grillbot.Services.Logger.LoggerMethods.LogEmbed;
using Microsoft.Extensions.Configuration;

namespace Grillbot.Services.Logger.LoggerMethods
{
    public class UserUpdated : LoggerMethodBase
    {
        public UserUpdated(DiscordSocketClient client, IConfiguration config) : base(client, config, null)
        {
        }

        public async Task Process(SocketUser userBefore, SocketUser userAfter)
        {
            var logEmbedBuilder = new LogEmbedBuilder("Uživatel si aktualizoval profil", LogEmbedType.UserUpdated);

            logEmbedBuilder
                .SetAuthor(userBefore)
                .SetFooter($"UserID: {userBefore.Id}")
                .SetTimestamp(true)
                .AddField("Změny", "---------------------------------------------");

            if(userBefore.DiscriminatorValue != userAfter.DiscriminatorValue)
            {
                logEmbedBuilder.AddField("Discord Tag", $"{userBefore.Discriminator} -> {userAfter.Discriminator}");
            }

            if(userBefore.Username != userAfter.Username)
            {
                logEmbedBuilder.AddField("Jméno", $"{userBefore.Username} -> {userAfter.Username}");
            }

            var loggerRoom = GetLoggerRoom();
            await loggerRoom.SendMessageAsync(embed: logEmbedBuilder.Build());
        }
    }
}