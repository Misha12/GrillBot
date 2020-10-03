using System.Threading.Tasks;
using Discord.WebSocket;
using Grillbot.Extensions;
using Grillbot.Models.Config.AppSettings;
using Grillbot.Services.Logger.LoggerMethods.LogEmbed;

namespace Grillbot.Services.Logger.LoggerMethods
{
    public class UserJoined : LoggerMethodBase
    {
        public UserJoined(DiscordSocketClient client, Configuration config) : base(client, config, null, null, null)
        {
        }

        public async Task ProcessAsync(SocketGuildUser user)
        {
            var logEmbedBuilder = new LogEmbedBuilder("Připojil se uživatel", LogEmbedType.UserJoined);

            var createdAt = user.CreatedAt.LocalDateTime.ToLocaleDatetime();

            logEmbedBuilder
                .SetAuthor(user)
                .SetFooter($"MemberID: {user.Id}")
                .AddField("Založen", createdAt)
                .AddField("Počet členů na serveru", user.Guild.MemberCount);

            await SendEmbedAsync(logEmbedBuilder).ConfigureAwait(false);
        }
    }
}
