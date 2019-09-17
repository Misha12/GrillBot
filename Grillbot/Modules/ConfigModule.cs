using Discord.Commands;
using Grillbot.Services.Config;
using Grillbot.Services.Config.Models;
using Grillbot.Services.Preconditions;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace Grillbot.Modules
{
    [Group("config")]
    [Name("Správa konfigurace")]
    [RequirePermissions("ModifyConfig")]
    public class ConfigModule : BotModuleBase
    {
        private Configuration Config { get; set; }
        private OptionsWriter OptionsWriter { get; set; }

        public ConfigModule(IOptions<Configuration> config, OptionsWriter optionsWriter)
        {
            Config = config.Value;
            OptionsWriter = optionsWriter;
        }

        [Command("get")]
        [Summary("Vrátí hodnotu konkrétní položky v configu.")]
        public async Task Get(string section)
        {
            var value = Config.GetValue(section);

            if(!string.IsNullOrEmpty(value))
                await ReplyAsync($"{section}```{value}```");
        }

        [Command("GetPermissionNames")]
        [Summary("Vypíše seznam všech sekcí, kterým lze nastavit oprávnění.")]
        public async Task GetPermissionSections()
        {
            var data = Config.MethodsConfig.GetPermissionNames();

            if(data.Count > 0)
            {
                data.Insert(0, "Seznam všech oprávnění:");
                data.Insert(1, "");

                data.Add("");
                data.Add("Ke každému oprávnění lze zavolat AddUser, RemoveUser, AddRole, RemoveRole, BanUser, UnbanUser a OnlyAdmins (true/false).");

                await ReplyAsync(string.Join("\n", data));
            }
        }

        [Command("SetPermissions")]
        public async Task SetPermissions(string section, string type, string value)
        {
            await DoAsync(async () =>
            {
                OptionsWriter.UpdateOptions(o => o.MethodsConfig.SetPermissions(section, type, value));
                await ReplyAsync($"Oprávnění k {section} aktualizována.");
            });
        }
    }
}
