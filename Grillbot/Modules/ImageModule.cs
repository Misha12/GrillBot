using Discord.Commands;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Grillbot.Services.Preconditions;

namespace Grillbot.Modules
{
    [IgnorePM]
    [Name("Nudes a další zajímavé fotky")]
    [DisabledCheck(RoleGroupName = "Images")]
    [RequireRoleOrAdmin(RoleGroupName = "Images")]
    public class ImageModule : BotModuleBase
    {
        private IConfiguration Config { get; }

        public ImageModule(IConfiguration configuration)
        {
            Config = configuration;
        }

        [Command("nudes")]
        public async Task SendNudeAsync() => await SendAsync("Nudes");

        [Command("notnudes")]
        public async Task SendNotNudesAsync() => await SendAsync("NotNudes");

        private async Task SendAsync(string category)
        {
            var config = Config.GetSection("MethodsConfig:Images");

            var files = Directory.GetFiles(config[$"{category}DataPath"])
                .Where(o => config.GetSection("AllowedImageTypes").GetChildren().Any(x => x.Value == Path.GetExtension(o)))
                .ToList();

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
