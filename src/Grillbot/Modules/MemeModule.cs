using Discord.Commands;
using System.Threading.Tasks;
using Grillbot.Services.MemeImages;
using Grillbot.Attributes;
using Grillbot.Models.Config.Dynamic;
using Grillbot.Extensions.Discord;
using Grillbot.Enums;
using System;
using System.Linq;
using Grillbot.Services.Duck;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace Grillbot.Modules
{
    [ModuleID("MemeModule")]
    [Name("Ostatní zbytečnosti")]
    public class MemeModule : BotModuleBase
    {
        public MemeModule(IServiceProvider provider) : base(provider: provider)
        {
        }

        [Command("nudes")]
        public async Task SendNudeAsync()
        {
            await SendAsync("nudes").ConfigureAwait(false);
        }

        [Command("notnudes")]
        public async Task SendNotNudesAsync()
        {
            await SendAsync("notnudes").ConfigureAwait(false);
        }

        private async Task SendAsync(string category)
        {
            using var service = GetService<MemeImagesService>();
            var content = await service.Service.GetRandomFileAsync(Context.Guild, category);

            if (content == null)
            {
                await ReplyAsync("Nemám žádný obrázek.");
                return;
            }

            await ReplyFileAsync(content, $"{category}.png");
        }

        [Command("peepolove")]
        public async Task PeepoloveAsync(Discord.IUser forUser = null)
        {
            if (forUser == null)
                forUser = Context.User;

            var config = await GetMethodConfigAsync<PeepoloveConfig>(null, "peepolove");

            using var service = GetService<MemeImagesService>();
            using var bitmap = await service.Service.CreatePeepoloveAsync(forUser, config);
            await ReplyImageAsync(bitmap, "peepolove.png");
        }

        [Command("peepoangry")]
        [Alias("pcbts", "peepoCantBelieveThisShit")]
        [Summary("PeepoAngry emote zírající na profilovku uživatele.")]
        public async Task PeepoAngryAsync(Discord.IUser forUser = null)
        {
            if (forUser == null)
                forUser = Context.User;

            var config = await GetMethodConfigAsync<PeepoAngryConfig>(null, "peepoangry");
            using var service = GetService<MemeImagesService>();
            using var bitmap = await service.Service.PeepoAngryAsync(forUser, config);
            await ReplyImageAsync(bitmap, "peepoangry.png");
        }

        [Command("grillhi"), Alias("hi")]
        public async Task GreetAsync()
        {
            await GreetAsync(null);
        }

        [Command("grillhi"), Alias("hi")]
        [Remarks("Možné formáty odpovědi jsou 'text', 'bin', nebo 'hex'.")]
        public async Task GreetAsync(string mode)
        {
            var config = await GetMethodConfigAsync<GreetingConfig>("", "grillhi");

            if (string.IsNullOrEmpty(mode))
                mode = config.OutputMode.ToString().ToLower();

            mode = char.ToUpper(mode[0]) + mode[1..];
            var availableModes = new[] { "Text", "Bin", "Hex" };

            if (!availableModes.Contains(mode)) return;

            var message = config.MessageTemplate.Replace("{person}", Context.User.GetShortName());

            switch (Enum.Parse<GreetingOutputModes>(mode))
            {
                case GreetingOutputModes.Bin:
                    message = ConvertToBinOrHexa(message, 2);
                    break;
                case GreetingOutputModes.Hex:
                    message = ConvertToBinOrHexa(message, 16);
                    break;
                case GreetingOutputModes.Text:
                    message = config.MessageTemplate.Replace("{person}", Context.User.Mention);
                    break;
            }

            await ReplyAsync(message);
        }

        [Command("grillhi"), Alias("hi")]
        [Remarks("Možné základy soustav odpovědi jsou 2, 8, 10, nebo 16.")]
        public async Task GreetAsync(int @base)
        {
            var supportedBases = new[] { 2, 8, 10, 16 };

            if (!supportedBases.Contains(@base)) return;

            var config = await GetMethodConfigAsync<GreetingConfig>("", "grillhi");

            var message = config.MessageTemplate.Replace("{person}", Context.User.GetFullName());
            var converted = ConvertToBinOrHexa(message, @base);

            await ReplyAsync(converted);
        }

        private string ConvertToBinOrHexa(string message, int @base)
        {
            return string.Join(" ", message.Select(o => Convert.ToString(o, @base)));
        }

        [Command("kachna", true)]
        [Alias("duck")]
        [Summary("Zjištění aktuálního stavu kachny.")]
        public async Task GetDuckInfoAsync()
        {
            try
            {
                using var duckLoader = GetService<DuckDataLoader>();
                var duckRenderer = duckLoader.Scope.ServiceProvider.GetService<DuckEmbedRenderer>();

                var config = await GetMethodConfigAsync<DuckConfig>("kachna", null);
                var duckData = await duckLoader.Service.GetDuckCurrentStateAsync(config);

                var embed = duckRenderer.RenderEmbed(duckData, Context.User, config);
                await ReplyAsync(embed: embed.Build());
            }
            catch(WebException ex)
            {
                await ReplyAsync(ex.Message);
            }
        }
    }
}
