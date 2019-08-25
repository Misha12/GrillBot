using Discord;
using Discord.Commands;
using Grillbot.Services.Preconditions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grillbot.Modules
{
    [IgnorePM]
    [Name("Správa rolí")]
    [DisabledCheck(RoleGroupName = "RoleManager")]
    [RequireRoleOrAdmin(RoleGroupName = "RoleManager")]
    public class RoleManagerModule : BotModuleBase
    {
        [Command("rolereport")]
        public async Task GetRoleReportAsync()
        {
            var roleInfoFields = new List<EmbedFieldBuilder>();

            foreach (var role in Context.Guild.Roles.Where(o => !o.IsEveryone && !o.IsManaged).OrderByDescending(o => o.Position))
            {
                var roleInfo = new StringBuilder()
                    .Append("Uživatelů: ").AppendLine($"**{role.Members.Count()}**")
                    .Append("Tagovatelná role: ").Append($"**{(role.IsMentionable ? "Ano" : "Ne")}**")
                    .ToString();

                roleInfoFields.Add(new EmbedFieldBuilder()
                {
                    Name = role.Name,
                    Value = roleInfo.ToString()
                });
            }

            const int roleMaxCount = DiscordService.MaxEmbedFields;
            for(int i = 0; i < (float)roleInfoFields.Count / roleMaxCount; i++)
            {
                var embed = new EmbedBuilder()
                    .WithColor(Color.Blue)
                    .WithFields(roleInfoFields.Skip(i * roleMaxCount).Take(roleMaxCount));

                await ReplyAsync(embed: embed.Build());
            }
        }

        [Command("rolereport")]
        public async Task GetRoleReportAsync(params string[] roleNameFields)
        {
            var embed = new EmbedBuilder();
            var role = Context.Guild.Roles.FirstOrDefault(o => o.Name == string.Join(" ", roleNameFields));

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

            var permissionItems = permissions.GetType().GetProperties()
                .Where(p => p.PropertyType == typeof(bool) && (bool)p.GetValue(permissions, null))
                .Select(o => o.Name)
                .ToList();

            return permissionItems.Count == 0 ? new List<string>() { "-" } : permissionItems;
        }
    }
}
