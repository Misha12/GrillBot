using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Grillbot.Services.Config;
using Grillbot.Services.Logger.LoggerMethods.LogEmbed;

namespace Grillbot.Services.Logger.LoggerMethods
{
    public class GuildMemberUpdated : LoggerMethodBase
    {
        public GuildMemberUpdated(DiscordSocketClient client, ConfigurationService configurationService) : base(client, null, null, null, null, configurationService)
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
            return guildUserBefore.Nickname != guildUserAfter.Nickname;
        }

        private bool IsBoostAdded(SocketGuildUser guildUserBefore, SocketGuildUser guildUserAfter)
        {
            var boosterRoleId = ConfigurationService.GetValue(Enums.GlobalConfigItems.ServerBoosterRoleId);

            if (string.IsNullOrEmpty(boosterRoleId))
                return false;

            var boosterRoleIdValue = Convert.ToUInt64(boosterRoleId);

            bool hasBefore = guildUserBefore.Roles.Any(o => o.Id == boosterRoleIdValue);
            bool hasAfter = guildUserAfter.Roles.Any(o => o.Id == boosterRoleIdValue);

            return !hasBefore && hasAfter;
        }

        private bool IsBoostRemoved(SocketGuildUser guildUserBefore, SocketGuildUser guildUserAfter)
        {
            var boosterRoleId = ConfigurationService.GetValue(Enums.GlobalConfigItems.ServerBoosterRoleId);

            if (string.IsNullOrEmpty(boosterRoleId))
                return false;

            var boosterRoleIdValue = Convert.ToUInt64(boosterRoleId);

            bool hasBefore = guildUserBefore.Roles.Any(o => o.Id == boosterRoleIdValue);
            bool hasAfter = guildUserAfter.Roles.Any(o => o.Id == boosterRoleIdValue);

            return hasBefore && !hasAfter;
        }

        private async Task BoostRemoved(SocketGuildUser user)
        {
            await SendBoostChange(user, "Uživatel na serveru již není ServerBooster.").ConfigureAwait(false);
        }

        private async Task BoostAdded(SocketGuildUser user)
        {
            await SendBoostChange(user, "Uživatel na serveru je nyní ServerBooster.").ConfigureAwait(false);
        }

        private async Task SendBoostChange(SocketGuildUser user, string embedMessage)
        {
            var logEmbedBuilder = new LogEmbedBuilder(embedMessage, LogEmbedType.BoostUpdated);

            logEmbedBuilder
                .SetAuthor(user)
                .SetFooter($"UserID: {user.Id}");

            await SendEmbedToAdminChannel(logEmbedBuilder).ConfigureAwait(false);
        }
    }
}
