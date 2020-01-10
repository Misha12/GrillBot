﻿using Discord;
using Discord.WebSocket;
using Grillbot.Extensions;
using Grillbot.Extensions.Discord;
using Grillbot.Repository.Entity;
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

            var channels = guild.Channels.Where(o => Config.MethodsConfig.TempUnverify.PreprocessRemoveAccess.Contains(o.Id.ToString())).ToList();

            foreach(var channel in channels)
            {
                var perms = new OverwritePermissions(sendMessages: PermValue.Deny);
                await channel.AddPermissionOverwriteAsync(user, perms).ConfigureAwait(false);
            }
        }

        private async Task RemoveAccessToPublicChannels(SocketGuildUser user, SocketGuild guild)
        {
            await guild.SyncGuildAsync().ConfigureAwait(false);

            foreach (var channel in guild.Channels.OfType<SocketTextChannel>())
            {
                if(channel is IChannel ic)
                {
                    var restChannelUser = await ic.GetUserAsync(user.Id).ConfigureAwait(false);

                    if(restChannelUser != null)
                    {
                        var perms = channel.GetPermissionOverwrite(restChannelUser);

                        if (perms.HasValue && perms.Value.SendMessages == PermValue.Deny)
                            continue;

                        perms = new OverwritePermissions(sendMessages: PermValue.Deny);
                        await channel.AddPermissionOverwriteAsync(restChannelUser, perms.Value).ConfigureAwait(false);
                    }
                }
            }
        }

        private async Task ReturnAccessToPublicChannels(SocketGuildUser user, SocketGuild guild)
        {
            await guild.SyncGuildAsync().ConfigureAwait(false);

            foreach (var channel in guild.Channels)
            {
                var overwrite = channel.GetPermissionOverwrite(user);

                if (overwrite != null)
                    await channel.RemovePermissionOverwriteAsync(user).ConfigureAwait(false);
            }
        }

        private List<ChannelOverride> GetChannelOverrides(SocketGuildUser user)
        {
            return user.Guild.Channels
                .Select(channel => new { channel.Id, overrides = channel.GetPermissionOverwrite(user) })
                .Where(channel => channel?.overrides != null)
                .Select(channel => new ChannelOverride(channel.Id, channel.overrides.Value))
                .ToList();
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
