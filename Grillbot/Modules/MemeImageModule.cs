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
    [RequirePermissions("MemeImages", BoosterAllowed = true, DisabledForPM = false)]
    public class MemeImageModule : BotModuleBase
    {
        private Configuration Config { get; }

        public MemeImageModule(IOptions<Configuration> configuration)
        {
            Config = configuration.Value;
        }

        [Command("nudes")]
        public async Task SendNudeAsync() => await SendAsync("Nudes").ConfigureAwait(false);

        [Command("notnudes")]
        public async Task SendNotNudesAsync() => await SendAsync("NotNudes").ConfigureAwait(false);

        private async Task SendAsync(string category)
        {
            var config = Config.MethodsConfig.MemeImages;

            var path = "";
            switch (category)
            {
                case "Nudes":
                    path = config.NudesDataPath;
                    break;
                case "NotNudes":
                    path = config.NotNudesDataPath;
                    break;
            }

            await DoAsync(async () =>
            {
                var files = Directory.GetFiles(path)
                    .Where(o => config.AllowedImageTypes.Any(x => x == Path.GetExtension(o)))
                    .ToList();

                if(files.Count == 0)
                    throw new ArgumentException("Nemám žádný obrázek.");

                var random = new Random();
                var randomValue = random.Next(files.Count);

                await Context.Channel.SendFileAsync(files[randomValue]).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }
    }
}
