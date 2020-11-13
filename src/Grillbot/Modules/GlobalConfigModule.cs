using Discord.Commands;
using Grillbot.Attributes;
using Grillbot.Enums;
using Grillbot.Services.Config;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grillbot.Modules
{
    [Group("globalConfig")]
    [ModuleID("GlobalConfigModule")]
    [Name("Globální konfigurace")]
    public class GlobalConfigModule : BotModuleBase
    {
        private ConfigurationService ConfigurationService { get; }

        public GlobalConfigModule(ConfigurationService configurationService)
        {
            ConfigurationService = configurationService;
        }

        [Command("keys")]
        [Summary("Získání konfiguračních klíčů pro DB")]
        public async Task GetKeysAsync()
        {
            var items = Enum.GetValues<GlobalConfigItems>();
            var formated = items.Select(o => o.ToString());

            await ReplyAsync($"```\n{string.Join(Environment.NewLine, formated)}```");
        }

        [Command("get")]
        [Summary("Získá konfigurační hodnotu pro zadaný klíč.")]
        public async Task GetAsync(GlobalConfigItems key)
        {
            var value = ConfigurationService.GetValue(key);

            if (string.IsNullOrEmpty(value))
                value = "null";

            if (value.Length >= Discord.DiscordConfig.MaxMessageSize)
            {
                using var stream = new MemoryStream(Encoding.UTF8.GetBytes(value));
                await Context.Channel.SendFileAsync(stream, $"{key}.txt");
                return;
            }

            await ReplyAsync($"```\n{value}```");
        }

        [Command("set")]
        [Summary("Uloží konfigurační hodnotu pro zadaný klíč.")]
        public async Task SetAsync(GlobalConfigItems key, string value)
        {
            if (string.Equals(value, "null", StringComparison.InvariantCultureIgnoreCase))
                value = null;

            await ConfigurationService.SetValueAsync(key, value);
            await ReplyAsync("Konfigurace uložena");
        }
    }
}
