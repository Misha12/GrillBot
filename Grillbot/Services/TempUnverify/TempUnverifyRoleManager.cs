using Discord.WebSocket;
using Grillbot.Database.Repository;
using Grillbot.Models.Config.Dynamic;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using NeoSmart.Unicode;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Services.TempUnverify
{
    public class TempUnverifyRoleManager : IDisposable
    {
        private ConfigRepository ConfigRepository { get; }

        public TempUnverifyRoleManager(ConfigRepository configRepository)
        {
            ConfigRepository = configRepository;
        }

        public async Task<List<SocketRole>> GetRolesToKeepAsync(string[] roleNamesToKeep, SocketGuildUser user, SocketGuild guild)
        {
            if (roleNamesToKeep == null || roleNamesToKeep.Length == 0)
                return new List<SocketRole>();

            var selfUnverifyConfig = ConfigRepository.FindConfig(guild.Id, "selfunverify", null)?.GetData<SelfUnverifyConfig>();

            if (selfUnverifyConfig == null)
                throw new InvalidOperationException("Neplatná konfigurace pro selfunverify.");

            roleNamesToKeep = roleNamesToKeep.Select(o => o.ToLower()).Distinct().ToArray();
            if (roleNamesToKeep.Length > selfUnverifyConfig.MaxRolesToKeep)
                throw new ValidationException($"Lze si ponechat maximálně následující počet rolí: {selfUnverifyConfig.MaxRolesToKeep}");

            var realRoleNamesToKeep = GetRealRoleNamesToKeep(roleNamesToKeep, selfUnverifyConfig.RolesToKeep, user);
            return user.Roles.Where(o => realRoleNamesToKeep.Contains(o.Name.ToLower())).ToList();
        }

        private List<string> GetRealRoleNamesToKeep(string[] roleNamesToKeep, Dictionary<string, List<string>> groups, SocketGuildUser user)
        {
            var realRoleNames = new List<string>();

            foreach (var roleName in roleNamesToKeep)
            {
                // If role is not in definition.
                if (!ExistsInRoleKeepDefinition(groups, roleName))
                    throw new ValidationException($"Role `{roleName.ToUpper()}` není ponechatelná role.");

                if (user.Roles.Any(o => string.Equals(o.Name, roleName, StringComparison.InvariantCultureIgnoreCase)))
                {
                    // User want keep role, which have.
                    // Role is in definitions. Can add to list and skip to next role.
                    realRoleNames.Add(roleName);
                    continue;
                }

                // User have not role, which want keep, we can search in groups.
                foreach (var group in groups)
                {
                    if (group.Value == null)
                        continue;

                    if (group.Value.Contains(roleName))
                        realRoleNames.Add(group.Key == "_" ? roleName : group.Key);
                }
            }

            return realRoleNames
                .Distinct()
                .ToList();
        }

        private bool ExistsInRoleKeepDefinition(Dictionary<string, List<string>> groups, string roleName)
        {
            if (groups.ContainsKey(roleName))
                return true;

            return groups.Values
                .Where(o => o != null)
                .Any(o => o.Contains(roleName));
        }

        public void Dispose()
        {
            ConfigRepository.Dispose();
        }
    }
}
