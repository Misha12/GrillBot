using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Grillbot.Exceptions;
using Grillbot.Extensions.Discord;
using Grillbot.Models.Embed;
using Grillbot.Services.Preconditions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grillbot.Modules
{
    [Group("roleinfo")]
    [RequirePermissions]
    [Name("Správa rolí")]
    public class RoleManagerModule : BotModuleBase
    {
        [Command("all")]
        public async Task GetCompleteRoleListAsync()
        {
            var fields = Context.Guild.Roles
                    .Where(o => !o.IsEveryone && !o.IsManaged)
                    .OrderByDescending(o => o.Position)
                    .Select(o => new { field = new EmbedFieldBuilder().WithName(o.Name).WithValue(CreateRoleInfo(o, false)), color = o.Color })
                    .ToList();

            const int roleMaxCount = EmbedBuilder.MaxFieldCount;
            var pagesCount = Math.Ceiling((float)fields.Count / roleMaxCount);

            for (int i = 0; i < pagesCount; i++)
            {
                var fieldList = fields.Skip(i * roleMaxCount).Take(roleMaxCount);

                var embed = new BotEmbed(Context.Message.Author)
                    .SetColor(fieldList.FirstOrDefault(o => o.color.RawValue != 0)?.color ?? Color.Blue)
                    .PrependFooter($"Strana {i + 1} z {pagesCount}")
                    .WithFields(fieldList.Select(o => o.field));

                await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
            }
        }

        [Command("")]
        public async Task GetTopRoleList()
        {
            var topRoles = Context.Guild.Roles.Where(o => !o.IsEveryone && !o.IsManaged)
                .OrderByDescending(o => o.Members.Count())
                .ThenByDescending(o => o.Position)
                .Take(EmbedBuilder.MaxFieldCount)
                .ToList();

            var embed = new BotEmbed(Context.Message.Author, topRoles.Find(o => o.Color.RawValue != 0)?.Color)
                .WithFields(topRoles.Select(o => new EmbedFieldBuilder().WithName(o.Name).WithValue(CreateRoleInfo(o, false))));

            await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
        }

        [Command("")]
        public async Task GetRoleReportAsync([Remainder] string roleName)
        {
            if (roleName == "all")
            {
                await GetCompleteRoleListAsync().ConfigureAwait(false);
                return;
            }

            var role = Context.Guild.Roles.FirstOrDefault(o => o.Name == roleName);

            if (role == null)
            {
                await ReplyAsync($"Roli `{roleName}` neznám.");
                return;
            }

            var embed = new BotEmbed(Context.Message.Author, role.Color)
                .WithFields(new EmbedFieldBuilder().WithName(role.Name).WithValue(CreateRoleInfo(role, true)));

            await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
        }

        private string CreateRoleInfo(SocketRole role, bool includePermissions)
        {
            var roleInfo = new StringBuilder()
                .Append("Počet uživatelů: ").AppendLine(role.Members.Count().ToString())
                .Append("Tagovatelná role: ").AppendLine(role.IsMentionable ? "Ano" : "Ne");

            if (includePermissions)
                roleInfo.Append("Oprávnění: ").AppendLine(string.Join(", ", role.Permissions.GetPermissionsNames()));

            return roleInfo.ToString();
        }
    }
}
