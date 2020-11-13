using System.Threading.Tasks;
using Discord.WebSocket;
using Grillbot.Extensions;
using Grillbot.Extensions.Discord;
using Grillbot.Services.Config;
using Grillbot.Services.Logger.LoggerMethods.LogEmbed;

namespace Grillbot.Services.Logger.LoggerMethods
{
    public class UserLeft : LoggerMethodBase
    {
        public UserLeft(DiscordSocketClient client, ConfigurationService config) : base(client, null, null, null, config)
        {
        }

        public async Task ProcessAsync(SocketGuildUser user)
        {
            var logEmbedBuilder = new LogEmbedBuilder("Uživatel opustil server.", LogEmbedType.UserLeft);
            var ban = await user.Guild.FindBanAsync(user);

            logEmbedBuilder = logEmbedBuilder
                .SetAuthor(user)
                .AddField("Počet členů na serveru", user.Guild.MemberCount, true)
                .AddField("Udělen ban", (ban != null).TranslateToCz(), true)
                .SetFooter($"MemberID: {user.Id}");

            if (!string.IsNullOrEmpty(ban?.Reason))
                logEmbedBuilder.AddField("Důvod banu", ban.Reason);

            await SendEmbedAsync(logEmbedBuilder).ConfigureAwait(false);
        }
    }
}
