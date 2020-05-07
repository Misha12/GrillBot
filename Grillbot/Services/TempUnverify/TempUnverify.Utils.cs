using Discord;
using Discord.WebSocket;
using Grillbot.Database.Entity;
using Grillbot.Extensions;
using Grillbot.Extensions.Discord;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Services.TempUnverify
{
    public partial class TempUnverifyService
    {
        private async Task FindAndToggleMutedRoleAsync(SocketGuildUser user, SocketGuild guild, bool set)
        {
            await guild.SyncGuildAsync();

            var mutedRole = guild.FindMutedRole();
            if (mutedRole == null)
                return; // Mute role not exists on this server.

            if (set)
                await user.SetRoleAsync(mutedRole);
            else
                await user.RemoveRoleAsync(mutedRole);
        }

        /// <summary>
        /// Remove access to channels where user can't see now, but after unverify can see.
        /// </summary>
        private async Task PreRemoveAccessToPublicChannels(SocketGuildUser user, SocketGuild guild)
        {
            await guild.SyncGuildAsync().ConfigureAwait(false);

            var configData = Factories.GetConfig(guild.Id);

            var channels = guild.Channels
                .OfType<SocketTextChannel>()
                .Where(o => configData.PreprocessRemoveAccess.Contains(o.Id.ToString()));

            foreach (var channel in channels)
            {
                var cantSee = channel.GetUser(user.Id) == null;
                if (cantSee) return;

                var perms = new OverwritePermissions(sendMessages: PermValue.Deny);
                await channel.AddPermissionOverwriteAsync(user, perms).ConfigureAwait(false);
            }
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

            var configData = Factories.GetConfig(guild.Id);

            var overwrites = guild.Channels
                .OfType<SocketTextChannel>()
                .Where(o => configData.PreprocessRemoveAccess.Contains(o.Id.ToString()) &&
                    !overrideExceptions.Any(x => x.ChannelIdSnowflake == o.Id))
                .Select(ch => new { channel = ch, overwrite = ch.GetPermissionOverwrite(user) })
                .Where(o => o.overwrite != null);

            foreach (var channel in overwrites)
            {
                await channel.channel.RemovePermissionOverwriteAsync(user).ConfigureAwait(false);
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
