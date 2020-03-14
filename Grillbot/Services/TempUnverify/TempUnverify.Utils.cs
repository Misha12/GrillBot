using Discord;
using Discord.WebSocket;
using Grillbot.Database.Entity;
using Grillbot.Extensions;
using Grillbot.Extensions.Discord;
using Grillbot.Services.Config.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Services.TempUnverify
{
    public partial class TempUnverifyService
    {
        /// <summary>
        /// Remove access to channels where user can't see now, but after unverify can see.
        /// </summary>
        private async Task PreRemoveAccessToPublicChannels(SocketGuildUser user, SocketGuild guild)
        {
            await guild.SyncGuildAsync().ConfigureAwait(false);

            var config = ConfigRepository.FindConfig(guild.Id, "unverify", "");
            var configData = config.GetData<TempUnverifyConfig>();

            var channels = guild.Channels
                .OfType<SocketTextChannel>()
                .Where(o => configData.PreprocessRemoveAccess.Contains(o.Id.ToString()));

            foreach (var channel in channels)
            {
                var canSee = channel.GetUser(user.Id) != null;
                if (!canSee) return;

                var perms = new OverwritePermissions(sendMessages: PermValue.Deny);
                await channel.AddPermissionOverwriteAsync(user, perms).ConfigureAwait(false);
            }
        }

        private async Task FindAndToggleMutedRole(SocketGuildUser user, SocketGuild guild, bool set)
        {
            await guild.SyncGuildAsync().ConfigureAwait(false);

            var mutedRole = guild.Roles
                .FirstOrDefault(o => string.Equals(o.Name, "muted", StringComparison.InvariantCultureIgnoreCase));

            if (mutedRole == null)
                return; // Mute role not exists on this server.

            if (set)
                await user.AddRoleAsync(mutedRole).ConfigureAwait(false);
            else
                await user.RemoveRoleAsync(mutedRole).ConfigureAwait(false);
        }

        private List<ChannelOverride> GetChannelOverrides(SocketGuildUser user)
        {
            return user.Guild.Channels
                .Select(channel => new { channel.Id, overrides = channel.GetPermissionOverwrite(user) })
                .Where(channel => channel?.overrides != null && (channel?.overrides?.AllowValue > 0 || channel?.overrides?.DenyValue > 0))
                .Select(channel => new ChannelOverride(channel.Id, channel.overrides.Value))
                .ToList();
        }

        private async Task RemoveOverwritesForPreprocessedChannels(SocketGuildUser user, SocketGuild guild,
            List<ChannelOverride> overrideExceptions)
        {
            await guild.SyncGuildAsync().ConfigureAwait(false);

            var config = ConfigRepository.FindConfig(guild.Id, "unverify", "");
            var configData = config.GetData<TempUnverifyConfig>();

            var channels = guild.Channels
                .OfType<SocketTextChannel>()
                .Where(o =>
                    configData.PreprocessRemoveAccess.Contains(o.Id.ToString()) &&
                    !overrideExceptions.Any(x => x.ChannelIdSnowflake == o.Id));

            foreach (var channel in channels)
            {
                var overwrites = channel.GetPermissionOverwrite(user);

                if (overwrites != null)
                    await channel.RemovePermissionOverwriteAsync(user).ConfigureAwait(false);
            }
        }

        private string GetFormatedPrivateMessage(SocketGuildUser user, TempUnverifyItem item, string reason, bool update)
        {
            var guildName = user.Guild.Name;
            var endDatetime = item.GetEndDatetime().ToLocaleDatetime();

            if (update)
                return $"Byl ti aktualizován čas pro odebrání práv na serveru **{guildName}**. Přístup ti bude navrácen **{endDatetime}**.";

            return $"Byly ti dočasně odebrány všechny práva na serveru **{guildName}**. Přístup ti bude navrácen **{endDatetime}**. Důvod: {reason}";
        }

        private string FormatMessageToChannel(List<SocketGuildUser> users, List<TempUnverifyItem> unverifyItems, string reason)
        {
            var userNames = string.Join(", ", users.Select(o => o.GetFullName()));
            var endDatetime = unverifyItems[0].GetEndDatetime().ToLocaleDatetime();

            return $"Dočasné odebrání přístupu pro uživatele **{userNames}** bylo dokončeno. Přístup bude navrácen **{endDatetime}**. Důvod: {reason}";
        }

        private string BuildChannelOverrideList(List<ChannelOverride> overrides, SocketGuild guild)
        {
            if (overrides.Count == 0)
                return "-";

            var builder = overrides.Select(o => guild.GetChannel(o.ChannelIdSnowflake)?.Name).Where(o => o != null);
            return string.Join(", ", builder);
        }
    }
}
