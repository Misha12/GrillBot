using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Grillbot.Attributes;
using Grillbot.Extensions;
using Grillbot.Extensions.Discord;
using Grillbot.Models.Embed;
using Grillbot.Models.Embed.PaginatedEmbed;
using Grillbot.Services;
using Grillbot.Services.Permissions.Preconditions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grillbot.Modules
{
    [Group("roleinfo")]
    [RequirePermissions]
    [Name("Správa rolí")]
    [ModuleID("RoleManagerModule")]
    public class RoleManagerModule : BotModuleBase
    {
        public RoleManagerModule(PaginationService pagination) : base(paginationService: pagination) { }

        [Command("all")]
        public async Task GetCompleteRoleListAsync()
        {
            var chunks = Context.Guild.Roles
                    .Where(o => !o.IsEveryone)
                    .OrderByDescending(o => o.Position)
                    .Select(o => new EmbedFieldBuilder().WithName(o.Name).WithValue(CreateRoleInfo(o, false)))
                    .SplitInParts(EmbedBuilder.MaxEmbedLength)
                    .ToList();

            var highestRoleWithColor = Context.Guild.Roles.FindHighestRoleWithColor();

            var paginatedEmbed = new PaginatedEmbed()
            {
                Color = highestRoleWithColor?.Color ?? Color.Blue,
                Pages = chunks.Select(ch => new PaginatedEmbedPage(null, ch.ToList())).ToList(),
                Thumbnail = Context.Guild.IconUrl,
                Title = "Informace o rolích",
                ResponseFor = Context.User
            };

            await SendPaginatedEmbedAsync(paginatedEmbed);
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
