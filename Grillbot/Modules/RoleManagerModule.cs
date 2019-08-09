using Discord;
using Discord.Commands;
using Grillbot.Services;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grillbot.Modules
{
    [Name("Správa rolí")]
    public class RoleManagerModule : BotModuleBase
    {
        [Command("rolereport")]
        [RequireRole(RoleGroupName = "RoleManager")]
        [DisabledCheck(RoleGroupName = "RoleManager")]
        public async Task GetRoleCounts()
        {
            var embed = new EmbedBuilder();

            foreach(var role in Context.Guild.Roles.Where(o => !o.IsEveryone && !o.IsManaged).OrderByDescending(o => o.Position))
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
    }
}
