using Discord.WebSocket;
using Grillbot.Extensions.Discord;
using Grillbot.Repository;
using Grillbot.Repository.Entity;
using Grillbot.Repository.Entity.UnverifyLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Services.TempUnverify
{
    public partial class TempUnverifyService
    {
        public async Task<string> RemoveAccessAsync(List<SocketGuildUser> users, string time, string data, SocketGuild guild,
            SocketUser fromUser)
        {
            CheckIfCanStartUnverify(users, guild, false);

            var reason = ParseReason(data);
            var unverifyTime = ParseUnverifyTime(time);
            var unverifiedPersons = new List<TempUnverifyItem>();

            using (var repository = new TempUnverifyRepository(Config))
            {
                foreach (var user in users)
                {
                    var person = await RemoveAccessAsync(repository, user, unverifyTime, reason, fromUser, guild, false)
                        .ConfigureAwait(false);
                    unverifiedPersons.Add(person);
                }
            }

            unverifiedPersons.ForEach(o => o.InitTimer(ReturnAccess));
            Data.AddRange(unverifiedPersons);

            return FormatMessageToChannel(users, unverifiedPersons, reason);
        }

        private async Task<TempUnverifyItem> RemoveAccessAsync(TempUnverifyRepository repository, SocketGuildUser user,
            long unverifyTime, string reason, SocketUser fromUser, SocketGuild guild, bool ignoreHigherRoles)
        {
            var rolesToRemove = user.Roles.Where(o => !o.IsEveryone && !o.IsManaged).ToList();

            if (ignoreHigherRoles)
            {
                var botMaxRolePosition = guild.GetUser(Client.CurrentUser.Id).Roles.Max(o => o.Position);
                rolesToRemove = rolesToRemove.Where(o => o.Position < botMaxRolePosition).ToList();
            }

            var rolesToRemoveNames = rolesToRemove.Select(o => o.Name).ToList();
            var overrides = GetChannelOverrides(user);

            var data = new UnverifyLogSet()
            {
                Overrides = overrides,
                Roles = rolesToRemoveNames,
                StartAt = DateTime.Now,
                TimeFor = unverifyTime.ToString(),
                Reason = reason
            };

            data.SetUser(user);
            await repository.LogOperationAsync(UnverifyLogOperation.Set, fromUser, guild, data).ConfigureAwait(false);

            await Logger.WriteAsync($"RemoveAccess {unverifyTime} secs (Roles: {string.Join(", ", rolesToRemoveNames)}, " +
                $"ExtraChannels: {string.Join(", ", overrides.Select(o => $"{o.ChannelId} => AllowVal: {o.AllowValue}, DenyVal => {o.DenyValue}"))}), " +
                $"{user.GetFullName()} ({user.Id}) Reason: {reason}").ConfigureAwait(false);

            await PreRemoveAccessToPublicChannels(user, guild).ConfigureAwait(false);
            await user.RemoveRolesAsync(rolesToRemove).ConfigureAwait(false);

            foreach (var channelOverride in overrides)
            {
                var channel = user.Guild.GetChannel(channelOverride.ChannelIdSnowflake);
                await channel?.RemovePermissionOverwriteAsync(user);
            }

            await RemoveAccessToPublicChannels(user, guild).ConfigureAwait(false);

            var unverify = await repository.AddItemAsync(rolesToRemoveNames, user.Id, user.Guild.Id, unverifyTime, overrides, reason).ConfigureAwait(false);

            var formatedPrivateMessage = GetFormatedPrivateMessage(user, unverify, reason, false);
            await user.SendPrivateMessageAsync(formatedPrivateMessage).ConfigureAwait(false);
            return unverify;
        }
    }
}
