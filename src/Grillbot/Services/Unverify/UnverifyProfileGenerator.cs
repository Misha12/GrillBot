using Discord.WebSocket;
using Grillbot.Database;
using Grillbot.Extensions.Discord;
using Grillbot.Models.Config.Dynamic;
using Grillbot.Services.Unverify.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Services.Unverify
{
    public class UnverifyProfileGenerator
    {
        private UnverifyTimeParser TimeParser { get; }
        private UnverifyReasonParser ReasonParser { get; }
        private DiscordSocketClient Discord { get; }
        private IGrillBotRepository GrillBotRepository { get; }

        public UnverifyProfileGenerator(UnverifyTimeParser timeParser, UnverifyReasonParser reasonParser, DiscordSocketClient discord, IGrillBotRepository grillBotRepository)
        {
            TimeParser = timeParser;
            ReasonParser = reasonParser;
            Discord = discord;
            GrillBotRepository = grillBotRepository;
        }

        public async Task<UnverifyUserProfile> CreateProfileAsync(SocketGuildUser user, SocketGuild guild, string time, string data, bool isSelfunverify, List<string> toKeep,
            SocketRole mutedRole)
        {
            var result = new UnverifyUserProfile()
            {
                StartDateTime = DateTime.Now,
                Reason = ReasonParser.Parse(data),
                EndDateTime = TimeParser.Parse(time),
                DestinationUser = user,
                IsSelfUnverify = isSelfunverify
            };

            var selfUnverifyConfig = await GetSelfunverifyConfigAsync(guild);

            if (isSelfunverify && selfUnverifyConfig == null)
                throw new InvalidOperationException("Neplatná konfigurace pro selfunverify");

            if (toKeep != null && selfUnverifyConfig != null)
            {
                toKeep = toKeep.Select(o => o.ToLower()).Distinct().ToList();
                if (toKeep.Count > selfUnverifyConfig.MaxRolesToKeep)
                    throw new ValidationException($"Lze si ponechat maximálně následující počet přístupů: {selfUnverifyConfig.MaxRolesToKeep}");
            }

            await SetRolesAsync(result, user, guild, isSelfunverify, toKeep, selfUnverifyConfig, mutedRole);
            SetChannels(result, user, toKeep, selfUnverifyConfig);

            return result;
        }

        private async Task SetRolesAsync(UnverifyUserProfile profile, SocketGuildUser user, SocketGuild guild, bool selfUnverify, List<string> toKeep,
            SelfUnverifyConfig selfUnverifyConfig, SocketRole mutedRole)
        {
            profile.RolesToRemove.AddRange(user.Roles);

            await FilterHigherRolesIfSelfunverifyAsync(profile, selfUnverify, guild);
            FilterUnavailableRoles(profile, mutedRole);

            if (toKeep == null || selfUnverifyConfig == null)
                return;

            foreach (var toKeepItem in toKeep)
            {
                CheckDefinitions(selfUnverifyConfig, toKeepItem);
                var role = profile.RolesToRemove.Find(o => string.Equals(o.Name, toKeepItem, StringComparison.InvariantCultureIgnoreCase));

                if (role != null)
                {
                    profile.RolesToKeep.Add(role);
                    profile.RolesToRemove.Remove(role);

                    continue;
                }

                foreach (var group in selfUnverifyConfig.RolesToKeep)
                {
                    if (group.Value == null)
                        continue;

                    if (group.Value.Contains(toKeepItem))
                    {
                        var roleItem = profile.RolesToRemove.Find(o => string.Equals(o.Name, group.Key == "_" ? toKeepItem : group.Key, StringComparison.InvariantCultureIgnoreCase));

                        if (roleItem != null)
                        {
                            profile.RolesToKeep.Add(roleItem);
                            profile.RolesToRemove.Remove(roleItem);
                        }
                    }
                }
            }
        }

        private async Task FilterHigherRolesIfSelfunverifyAsync(UnverifyUserProfile profile, bool isSelfunverify, SocketGuild guild)
        {
            if (!isSelfunverify)
                return;

            var botUser = await guild.GetUserFromGuildAsync(Discord.CurrentUser.Id);
            var botRolePosition = botUser.Roles.Max(o => o.Position);

            var rolesToKeep = profile.RolesToRemove.Where(o => o.Position >= botRolePosition).ToList();

            profile.RolesToKeep.AddRange(rolesToKeep);
            profile.RolesToRemove.RemoveAll(o => rolesToKeep.Any(x => x.Id == o.Id));
        }

        private void FilterUnavailableRoles(UnverifyUserProfile profile, SocketRole mutedRole)
        {
            var unavailableRoles = profile.RolesToRemove.FindAll(o => o.IsEveryone || o.IsManaged);
            unavailableRoles.RemoveAll(o => o.IsEveryone); // Ignore everyone role.

            if (mutedRole != null)
                unavailableRoles = unavailableRoles.FindAll(o => o.Id != mutedRole.Id);

            profile.RolesToKeep.AddRange(unavailableRoles);
            profile.RolesToRemove.RemoveAll(o => o.IsEveryone || unavailableRoles.Any(x => x.Id == o.Id));
        }

        private async Task<SelfUnverifyConfig> GetSelfunverifyConfigAsync(SocketGuild guild)
        {
            var config = await GrillBotRepository.ConfigRepository.FindConfigAsync(guild.Id, "selfunverify", null);
            return config?.GetData<SelfUnverifyConfig>();
        }

        private void SetChannels(UnverifyUserProfile profile, SocketGuildUser user, List<string> toKeep, SelfUnverifyConfig selfUnverifyConfig)
        {
            var channels = user.Guild.Channels
                .Select(channel => new ChannelOverwrite(channel, channel.GetPermissionOverwrite(user)))
                .Where(channel => channel.Permissions != null && (channel.AllowValue > 0 || channel.DenyValue > 0))
                .ToList();

            profile.ChannelsToRemove.AddRange(channels);

            if (toKeep == null || selfUnverifyConfig == null)
                return;

            foreach (var itemToKeep in toKeep)
            {
                CheckDefinitions(selfUnverifyConfig, itemToKeep);
                var overwrite = profile.ChannelsToRemove.Find(o => o.Channel.Name.Equals(itemToKeep, StringComparison.InvariantCultureIgnoreCase));

                if (overwrite != null)
                {
                    profile.ChannelsToKeep.Add(overwrite);
                    profile.ChannelsToRemove.RemoveAll(o => o.Channel.Id == overwrite.Channel.Id);
                }
            }
        }

        private bool ExistsInKeepDefinition(Dictionary<string, List<string>> groups, string name)
        {
            if (groups.ContainsKey(name))
                return true;

            return groups.Values.Any(o => o != null && o.Contains(name));
        }

        private void CheckDefinitions(SelfUnverifyConfig selfUnverifyConfig, string name)
        {
            if (!ExistsInKeepDefinition(selfUnverifyConfig.RolesToKeep, name))
                throw new ValidationException($"`{name.ToUpper()}` není ponechatelné.");
        }
    }
}
