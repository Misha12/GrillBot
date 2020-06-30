using Discord;
using Discord.WebSocket;
using Grillbot.Extensions.Discord;
using Grillbot.Database.Entity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Grillbot.Database.Repository;

namespace Grillbot.Services.TempUnverify
{
    public partial class TempUnverifyService
    {
        public async Task<string> RemoveAccessAsync(List<SocketGuildUser> users, string time, string data, SocketGuild guild,
            SocketUser fromUser, bool ignoreHigherRoles = false)
        {
            using var scope = Provider.CreateScope();
            using var checker = scope.ServiceProvider.GetService<TempUnverifyChecker>();

            foreach (var user in users)
            {
                checker.Validate(user, guild, false);
            }

            var reason = scope.ServiceProvider.GetService<TempUnverifyReasonParser>().Parse(data);
            var unverifyTime = scope.ServiceProvider.GetService<TempUnverifyTimeParser>().Parse(time);
            var unverifiedPersons = new List<TempUnverifyItem>();

            foreach (var user in users)
            {
                var person = await RemoveAccessAsync(user, unverifyTime, reason, fromUser, guild, ignoreHigherRoles, null);
                unverifiedPersons.Add(person);
            }

            unverifiedPersons.ForEach(o => o.InitTimer(ReturnAccess));
            Data.AddRange(unverifiedPersons);

            return FormatMessageToChannel(users, unverifiedPersons, reason);
        }

        private async Task<TempUnverifyItem> RemoveAccessAsync(SocketGuildUser user, int unverifyTime, string reason,
            SocketUser fromUser, SocketGuild guild, bool ignoreHigherRoles, string[] subjects)
        {
            using var scope = Provider.CreateScope();
            var unverifyConfig = GetConfig(guild);

            var rolesToRemove = user.Roles
                .Where(o => !o.IsEveryone && !o.IsManaged && o.Id != unverifyConfig.MutedRoleID)
                .ToList(); // Ignore Muted role.

            if (ignoreHigherRoles)
            {
                var botUser = await guild.GetUserFromGuildAsync(Client.CurrentUser.Id);
                var botMaxRolePosition = botUser.Roles.Max(o => o.Position);
                rolesToRemove = rolesToRemove.Where(o => o.Position < botMaxRolePosition).ToList();
            }

            using var roleManager = scope.ServiceProvider.GetService<TempUnverifyRoleManager>();
            var rolesToKeep = await roleManager.GetRolesToKeepAsync(subjects, user, guild);

            if (rolesToKeep.Count > 0)
                rolesToRemove = rolesToRemove.Where(o => !rolesToKeep.Contains(o)).ToList();

            var rolesToRemoveIDs = rolesToRemove.Select(o => o.Id).ToList();
            var overrides = GetChannelOverrides(user);

            using var logService = scope.ServiceProvider.GetService<TempUnverifyLogService>();
            logService.LogSet(overrides, rolesToRemoveIDs, unverifyTime, reason, user, fromUser, guild, ignoreHigherRoles, rolesToKeep);

            await FindAndToggleMutedRoleAsync(user, guild, unverifyConfig.MutedRoleID, true);
            await user.RemoveRolesAsync(rolesToRemove); // Remove all roles for user.

            // Remove all extra channel permissions.
            foreach (var channelOverride in overrides)
            {
                var channel = user.Guild.GetChannel(channelOverride.ChannelIdSnowflake);

                // Where had access, now not have.
                await channel?.AddPermissionOverwriteAsync(user, new OverwritePermissions(viewChannel: PermValue.Deny));
            }

            using var repository = scope.ServiceProvider.GetService<TempUnverifyRepository>();
            var unverify = await repository
                .AddItemAsync(rolesToRemoveIDs, user.Id, user.Guild.Id, unverifyTime, overrides, reason);

            var formatedPrivateMessage = GetFormatedPrivateMessage(user, unverify, reason, false);
            await user.SendPrivateMessageAsync(formatedPrivateMessage).ConfigureAwait(false);
            return unverify;
        }
    }
}
