using Discord;
using System.Collections.Generic;
using System.Linq;

namespace Grillbot.Extensions.Discord
{
    public static class GuildPermissionsExtensions
    {
        public static List<string> GetPermissionsNames(this GuildPermissions permissions)
        {
            if (permissions.Administrator)
                return new List<string>() { "Administrator" };

            var permissionItems = permissions.GetType().GetProperties()
                .Where(p => p.PropertyType == typeof(bool) && (bool)p.GetValue(permissions, null))
                .Select(o => o.Name)
                .ToList();

            return permissionItems.Count == 0 ? new List<string>() { "-" } : permissionItems;
        }
    }
}
