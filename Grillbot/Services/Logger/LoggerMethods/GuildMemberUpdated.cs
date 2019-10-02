using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Grillbot.Services.Config.Models;
using Grillbot.Services.Logger.LoggerMethods.LogEmbed;

namespace Grillbot.Services.Logger.LoggerMethods
{
    public class GuildMemberUpdated : LoggerMethodBase
    {
        public GuildMemberUpdated(DiscordSocketClient client, Configuration config) : base(client, config, null, null)
        {
        }

        public async Task<bool> ProcessAsync(SocketGuildUser guildUserBefore, SocketGuildUser guildUserAfter)
        {
            if (!IsChangeDetected(guildUserBefore, guildUserAfter)) return false;

            var logEmbedBuilder = new LogEmbedBuilder("Uživatel na serveru byl aktualizován.", LogEmbedType.GuildMemberUpdated);

            logEmbedBuilder
                .SetAuthor(guildUserBefore)
                .SetFooter($"UserID: {guildUserBefore.Id}")
                .SetTimestamp(true)
                .AddField("Změny", "---------------------------------------------");

            if(guildUserBefore.Nickname != guildUserAfter.Nickname)
            {
                if (string.IsNullOrEmpty(guildUserBefore.Nickname) && !string.IsNullOrEmpty(guildUserAfter.Nickname))
                    logEmbedBuilder.AddField("Server alias", $"(None) -> {guildUserAfter.Nickname}");
                else if(!string.IsNullOrEmpty(guildUserBefore.Nickname) && string.IsNullOrEmpty(guildUserAfter.Nickname))
                    logEmbedBuilder.AddField("Server alias", $"{guildUserBefore.Nickname} -> (None)");
                else
                    logEmbedBuilder.AddField("Server alias", $"{guildUserBefore.Nickname} -> {guildUserAfter.Nickname}");
            }

            var loggerRoom = GetLoggerRoom();
            await loggerRoom.SendMessageAsync(embed: logEmbedBuilder.Build());
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
    }
}
