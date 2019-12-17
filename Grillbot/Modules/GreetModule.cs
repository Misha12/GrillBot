using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;
using Grillbot.Services.Preconditions;
using Grillbot.Services.Config.Models;
using Microsoft.Extensions.Options;
using Grillbot.Extensions.Discord;

namespace Grillbot.Modules
{
    [Name("Pozdrav bota")]
    [RequirePermissions("Greeting", BoosterAllowed = true)]
    public class GreetModule : BotModuleBase
    {
        private Configuration Config { get; }

        public GreetModule(IOptions<Configuration> config)
        {
            Config = config.Value;
        }

        [Command("grillhi"), Alias("hojkashi", "hi")]
        public async Task GreetAsync() => await GreetAsync(Config.MethodsConfig.Greeting.OutputMode.ToString().ToLower()).ConfigureAwait(false);

        [Command("grillhi"), Alias("hojkashi", "hi")]
        [Remarks("Možné formáty odpovědi jsou 'text', 'bin', nebo 'hex'.")]
        public async Task GreetAsync(string mode)
        {
            mode = char.ToUpper(mode[0]) + mode.Substring(1);
            var availableModes = new[] { "Text", "Bin", "Hex" };

            if (!availableModes.Contains(mode)) return;
            var messageTemplate = Config.MethodsConfig.Greeting.MessageTemplate;

            var message = messageTemplate.Replace("{person}", Context.User.GetShortName());

            switch (Enum.Parse<GreetingOutputModes>(mode))
            {
                case GreetingOutputModes.Bin:
                    message = ConvertToBinOrHexa(message, 2);
                    break;
                case GreetingOutputModes.Hex:
                    message = ConvertToBinOrHexa(message, 16);
                    break;
                case GreetingOutputModes.Text:
                    message = messageTemplate.Replace("{person}", Context.User.Mention);
                    break;
            }

            await ReplyAsync(message).ConfigureAwait(false);
        }

        [Command("grillhi"), Alias("hojkashi", "hi")]
        [Remarks("Možné základy soustav odpovědi jsou 2, 8, 10, nebo 16.")]
        public async Task GreetAsync(int @base)
        {
            var supportedBases = new[] { 2, 8, 10, 16 };

            if (!supportedBases.Contains(@base)) return;

            var messageTemplate = Config.MethodsConfig.Greeting.MessageTemplate;
            var message = messageTemplate.Replace("{person}", Context.User.GetFullName());
            var converted = ConvertToBinOrHexa(message, @base);

            await ReplyAsync(converted).ConfigureAwait(false);
        }

        private string ConvertToBinOrHexa(string message, int @base)
        {
            return string.Join(" ", message.Select(o => Convert.ToString(o, @base)));
        }
    }
}
