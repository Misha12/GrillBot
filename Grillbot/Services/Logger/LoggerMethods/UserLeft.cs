using System.Threading.Tasks;
using Discord.WebSocket;
using Grillbot.Models;
using Grillbot.Services.Config.Models;
using Grillbot.Services.Logger.LoggerMethods.LogEmbed;

namespace Grillbot.Services.Logger.LoggerMethods
{
    public class UserLeft : LoggerMethodBase
    {
        public UserLeft(DiscordSocketClient client, Configuration config, TopStack stack) : base(client, config, null, null, null, stack)
        {
        }

        public async Task ProcessAsync(SocketGuildUser user)
        {
            var logEmbedBuilder = new LogEmbedBuilder("Uživatel opustil server.", LogEmbedType.UserLeft);

            logEmbedBuilder
                .SetAuthor(user)
                .AddField("Počet členů na serveru", user.Guild.MemberCount)
                .SetFooter($"MemberID: {user.Id}")
                .SetTimestamp(true);

            var loggerRoom = GetLoggerRoom();
            var result = await loggerRoom.SendMessageAsync(embed: logEmbedBuilder.Build());
            var info = $"{user.Username}#{user.Discriminator}";

            TopStack.Add(result, info);
        }
    }
}
