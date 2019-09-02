using Discord;
using Discord.Commands;
using Discord.WebSocket;
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
        public async Task GetCompleteRoleListAsync()
        {
            var roleInfoFields = new List<EmbedFieldBuilder>();

            foreach (var role in Context.Guild.Roles.Where(o => !o.IsEveryone && !o.IsManaged).OrderByDescending(o => o.Position))
            {
                var roleInfo = CreateRoleInfo(role, false);

                roleInfoFields.Add(new EmbedFieldBuilder()
                {
                    Name = role.Name,
                    Value = roleInfo.ToString()
                });
            }

            const int roleMaxCount = EmbedBuilder.MaxFieldCount;
            for(int i = 0; i < (float)roleInfoFields.Count / roleMaxCount; i++)
            {
                var embed = new EmbedBuilder()
                    .WithColor(Color.Blue)
                    .WithFields(roleInfoFields.Skip(i * roleMaxCount).Take(roleMaxCount))
                    .WithFooter($"Strana {i} | Odpověď pro {GetUsersShortName(Context.Message.Author)}")
                    .WithCurrentTimestamp();

                await ReplyAsync(embed: embed.Build());
            }
        }

        [Command("roleinfo")]
        public async Task GetTopRoleList()
        {
            var topRoles = Context.Guild.Roles.Where(o => !o.IsEveryone && !o.IsManaged)
                .OrderByDescending(o => o.Members.Count())
                .ThenByDescending(o => o.Position)
                .Take(EmbedBuilder.MaxFieldCount)
                .ToList();

            var embedBuilder = new EmbedBuilder() { Color = topRoles[0].Color };

            foreach(var role in topRoles)
            {
                embedBuilder.AddField(o => o.WithName(role.Name).WithValue(CreateRoleInfo(role, false)));
            }

            embedBuilder
                .WithCurrentTimestamp()
                .WithFooter($"Odpověď pro {GetUsersShortName(Context.Message.Author)}");

            await ReplyAsync(embed: embedBuilder.Build());
        }

        [Command("roleinfo")]
        public async Task GetRoleReportAsync([Remainder] string roleName)
        {
            if(roleName == "all")
            {
                await GetCompleteRoleListAsync();
                return;
            }

            var embed = new EmbedBuilder();
            var role = Context.Guild.Roles.FirstOrDefault(o => o.Name == roleName);

            if (role == null)
            {
                await ReplyAsync("Takovou roli neznám.");
                return;
            }

            embed.Color = role.Color;
            embed.AddField(o => o.WithName(role.Name).WithValue(CreateRoleInfo(role, true)));

            embed
                .WithCurrentTimestamp()
                .WithFooter($"Odpověď pro {GetUsersShortName(Context.Message.Author)}");

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

        private string CreateRoleInfo(SocketRole role, bool includePermissions)
        {
            var roleInfo = new StringBuilder()
                .Append("Počet uživatelů: ").AppendLine(role.Members.Count().ToString())
                .Append("Tagovatelná role: ").AppendLine(role.IsMentionable ? "Ano" : "Ne");

            if(includePermissions)
            {
                roleInfo.Append("Oprávnění: ").AppendLine(string.Join(", ", GetPermissionNames(role.Permissions)));
            }

            return roleInfo.ToString();
        }
    }
}
