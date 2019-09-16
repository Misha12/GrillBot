using Discord.Commands;
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

        public ConfigModule(IOptions<Configuration> config)
        {
            Config = config.Value;
        }

        [Command("get")]
        [Summary("Vrátí hodnotu konkrétní položky v configu.")]
        public async Task Get(string section)
        {
            var value = Config.GetValue(section);

            if(!string.IsNullOrEmpty(value))
                await ReplyAsync($"{section}```{value}```");
        }
    }
}
