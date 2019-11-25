using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Grillbot.Models;
using Grillbot.Services.Config.Models;
using Grillbot.Services.Logger.LoggerMethods.LogEmbed;

namespace Grillbot.Services.Logger.LoggerMethods
{
    public class GuildMemberUpdated : LoggerMethodBase
    {
        public GuildMemberUpdated(DiscordSocketClient client, Configuration config, TopStack stack) : base(client, config, null, null, null, stack)
        {
        }

        public async Task<bool> ProcessAsync(SocketGuildUser guildUserBefore, SocketGuildUser guildUserAfter)
        {
            if (IsBoostAdded(guildUserBefore, guildUserAfter))
            {
                await BoostAdded(guildUserAfter).ConfigureAwait(false);
                return true;
            }
            else if (IsBoostRemoved(guildUserBefore, guildUserAfter))
            {
                await BoostRemoved(guildUserAfter).ConfigureAwait(false);
                return true;
            }

            if (!IsChangeDetected(guildUserBefore, guildUserAfter)) return false;

            var logEmbedBuilder = new LogEmbedBuilder("Uživatel na serveru byl aktualizován.", LogEmbedType.GuildMemberUpdated);

            logEmbedBuilder
                .SetAuthor(guildUserBefore)
                .SetFooter($"UserID: {guildUserBefore.Id}")
                .SetTimestamp(true)
                .AddField("Změny", "---------------------------------------------");

            if (guildUserBefore.Nickname != guildUserAfter.Nickname)
            {
                if (string.IsNullOrEmpty(guildUserBefore.Nickname) && !string.IsNullOrEmpty(guildUserAfter.Nickname))
                    logEmbedBuilder.AddField("Server alias", $"(None) -> {guildUserAfter.Nickname}");
                else if (!string.IsNullOrEmpty(guildUserBefore.Nickname) && string.IsNullOrEmpty(guildUserAfter.Nickname))
                    logEmbedBuilder.AddField("Server alias", $"{guildUserBefore.Nickname} -> (None)");
                else
                    logEmbedBuilder.AddField("Server alias", $"{guildUserBefore.Nickname} -> {guildUserAfter.Nickname}");
            }

            await SendEmbedAsync(logEmbedBuilder).ConfigureAwait(false);
            return true;
        }

        private bool IsChangeDetected(SocketGuildUser guildUserBefore, SocketGuildUser guildUserAfter)
        {
            var changes = new[]
            {
                guildUserBefore.Nickname != guildUserAfter.Nickname
            };

            return changes.Any(o => o);
        }

        private bool IsBoostAdded(SocketGuildUser guildUserBefore, SocketGuildUser guildUserAfter)
        {
            bool hasBefore = Config.Discord.IsBooster(guildUserBefore.Roles);
            bool hasAfter = Config.Discord.IsBooster(guildUserAfter.Roles);

            return !hasBefore && hasAfter;
        }

        private bool IsBoostRemoved(SocketGuildUser guildUserBefore, SocketGuildUser guildUserAfter)
        {
            bool hasBefore = Config.Discord.IsBooster(guildUserBefore.Roles);
            bool hasAfter = Config.Discord.IsBooster(guildUserAfter.Roles);

            return hasBefore && !hasAfter;
        }

        private async Task BoostRemoved(SocketGuildUser user)
        {
            var logEmbedBuilder = new LogEmbedBuilder("Uživatel na serveru již není ServerBooster.", LogEmbedType.BoostUpdated);

            logEmbedBuilder
                .SetAuthor(user)
                .SetFooter($"UserID: {user.Id}")
                .SetTimestamp(true);

            await SendEmbedAsync(logEmbedBuilder).ConfigureAwait(false);
        }

        private async Task BoostAdded(SocketGuildUser user)
        {
            var logEmbedBuilder = new LogEmbedBuilder("Uživatel na serveru je nyní ServerBooster.", LogEmbedType.BoostUpdated);

            logEmbedBuilder
                .SetAuthor(user)
                .SetFooter($"UserID: {user.Id}")
                .SetTimestamp(true);

            await SendEmbedAsync(logEmbedBuilder).ConfigureAwait(false);
        }
    }
}
