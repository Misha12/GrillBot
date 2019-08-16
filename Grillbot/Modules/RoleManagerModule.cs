using Discord;
using Discord.Commands;
using Grillbot.Services;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grillbot.Modules
{
    [Name("Správa rolí")]
    public class RoleManagerModule : BotModuleBase
    {
        [Command("rolereport")]
        [RequireRoleOrAdmin(RoleGroupName = "RoleManager")]
        [DisabledCheck(RoleGroupName = "RoleManager")]
        public async Task GetRoleCounts()
        {
            var embed = new EmbedBuilder() { Color = Color.Blue };

            foreach (var role in Context.Guild.Roles.Where(o => !o.IsEveryone && !o.IsManaged).OrderByDescending(o => o.Position))
            {
                var roleInfo = new StringBuilder()
                    .Append("Počet uživatelů: ").AppendLine(role.Members.Count().ToString())
                    .Append("Tagovatelná role: ").AppendLine(role.IsMentionable ? "Ano" : "Ne")
                    .ToString();

                embed.AddField(o =>
                {
                    o.Name = role.Name;
                    o.Value = roleInfo;
                });
            }

            await ReplyAsync(embed: embed.Build());
        }

        [Command("rolereport")]
        [RequireRoleOrAdmin(RoleGroupName = "RoleManager")]
        [DisabledCheck(RoleGroupName = "RoleManager")]
        public async Task GetRoleReport(string rolename)
        {
            var embed = new EmbedBuilder();
            var role = Context.Guild.Roles.FirstOrDefault(o => o.Name == rolename);

            if (role == null)
            {
                await ReplyAsync("Takovou roli neznám.");
                return;
            }

            var roleInfo = new StringBuilder()
                .Append("Počet uživatelů: ").AppendLine(role.Members.Count().ToString())
                .Append("Tagovatelná role: ").AppendLine(role.IsMentionable ? "Ano" : "Ne")
                .Append("Oprávnění: ").AppendLine(string.Join(", ", GetPermissionNames(role.Permissions)))
                .ToString();

            embed.AddField(o =>
            {
                o.Name = role.Name;
                o.Value = roleInfo;
            });

            await ReplyAsync(embed: embed.Build());
        }

        public List<string> GetPermissionNames(GuildPermissions permissions)
        {
            if (permissions.Administrator)
                return new List<string>() { "Administrator" };

            return permissions.GetType().GetProperties()
                .Where(p => p.PropertyType == typeof(bool) && (bool)p.GetValue(permissions, null))
                .Select(o => o.Name)
                .ToList();
        }
    }
}
