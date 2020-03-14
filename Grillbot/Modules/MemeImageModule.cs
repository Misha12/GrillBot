using Discord.Commands;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Grillbot.Services.Preconditions;
using Grillbot.Services.Config.Models;
using Microsoft.Extensions.Options;
using Grillbot.Database.Repository;

namespace Grillbot.Modules
{
    [RequirePermissions]
    [Name("Nudes a další zajímavé fotky")]
    public class MemeImageModule : BotModuleBase
    {
        public MemeImageModule(IOptions<Configuration> configuration, ConfigRepository repository) :
            base(configuration, repository)
        {
        }

        [Command("nudes")]
        public async Task SendNudeAsync() => await SendAsync("nudes").ConfigureAwait(false);

        [Command("notnudes")]
        public async Task SendNotNudesAsync() => await SendAsync("notnudes").ConfigureAwait(false);

        private async Task SendAsync(string category)
        {
            var config = GetMethodConfig<MemeImagesConfig>("", category);

            await DoAsync(async () =>
            {
                var files = Directory.GetFiles(config.Path)
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
