using Discord.Commands;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Grillbot.Services.Preconditions;
using Grillbot.Services.Config.Models;
using Microsoft.Extensions.Options;

namespace Grillbot.Modules
{
    [Name("Nudes a další zajímavé fotky")]
    [RequirePermissions("MemeImages", BoosterAllowed = true)]
    public class ImageModule : BotModuleBase
    {
        private Configuration Config { get; }

        public ImageModule(IOptions<Configuration> configuration)
        {
            Config = configuration.Value;
        }

        [Command("nudes")]
        public async Task SendNudeAsync() => await SendAsync("Nudes");

        [Command("notnudes")]
        public async Task SendNotNudesAsync() => await SendAsync("NotNudes");

        private async Task SendAsync(string category)
        {
            var config = Config.MethodsConfig.MemeImages;

            var path = "";
            switch(category)
            {
                case "Nudes":
                    path = config.NudesDataPath;
                    break;
                case "NotNudes":
                    path = config.NotNudesDataPath;
                    break;
            }

            var files = Directory.GetFiles(path).Where(o => config.AllowedImageTypes.Any(x => x == Path.GetExtension(o))).ToList();

            if (files.Any())
            {
                var random = new Random();
                var randomValue = random.Next(files.Count);

                await Context.Channel.SendFileAsync(files[randomValue]);
            }
            else
            {
                await ReplyAsync("Nemám žádný obrázek");
            }
        }
    }
}
