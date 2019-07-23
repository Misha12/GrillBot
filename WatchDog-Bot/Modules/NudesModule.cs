using Discord.Commands;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WatchDog_Bot.Services;

namespace WatchDog_Bot.Modules
{
    [Name("Prostě nudes")]
    public class NudesModule : BotModuleBase
    {
        private IConfigurationRoot Config { get; }

        public NudesModule(IConfigurationRoot configuration)
        {
            Config = configuration;
        }

        [Command("nudes")]
        [RequireRole(RoleGroupName = "NudeImages")]
        public async Task SendNude() => await Send("Nudes");

        [Command("notnudes")]
        [RequireRole(RoleGroupName = "NudeImages")]
        public async Task SendNotNudes() => await Send("NotNudes");

        private async Task Send(string category)
        {
            var config = Config.GetSection("MethodsConfig:NudeImages");

            var files = Directory.GetFiles(config[$"{category}DataPath"])
                .Where(o => config.GetSection("AllowedImageTypes").GetChildren().Any(x => x.Value == Path.GetExtension(o)))
                .ToList();

            var random = new Random();
            var randomValue = random.Next(files.Count);

            await Context.Channel.SendFileAsync(files[randomValue]);
        }
    }
}
