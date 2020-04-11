using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;
using Grillbot.Services.Preconditions;
using Microsoft.Extensions.Options;
using Grillbot.Extensions.Discord;
using Grillbot.Database.Repository;
using Grillbot.Models.Config.AppSettings;
using Grillbot.Models.Config.Dynamic;

namespace Grillbot.Modules
{
    [RequirePermissions]
    [Name("Pozdrav bota")]
    public class GreetModule : BotModuleBase
    {
        public GreetModule(IOptions<Configuration> config, ConfigRepository repository) : base(config, repository)
        {
        }

        [Command("grillhi"), Alias("hojkashi", "hi")]
        public async Task GreetAsync() => await GreetAsync(null).ConfigureAwait(false);

        [Command("grillhi"), Alias("hojkashi", "hi")]
        [Remarks("Možné formáty odpovědi jsou 'text', 'bin', nebo 'hex'.")]
        public async Task GreetAsync(string mode)
        {
            var config = GetMethodConfig<GreetingConfig>("", "grillhi");

            if (string.IsNullOrEmpty(mode))
                mode = config.OutputMode.ToString().ToLower();

            mode = char.ToUpper(mode[0]) + mode.Substring(1);
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

            await ReplyAsync(message).ConfigureAwait(false);
        }

        [Command("grillhi"), Alias("hojkashi", "hi")]
        [Remarks("Možné základy soustav odpovědi jsou 2, 8, 10, nebo 16.")]
        public async Task GreetAsync(int @base)
        {
            var supportedBases = new[] { 2, 8, 10, 16 };

            if (!supportedBases.Contains(@base)) return;

            var config = GetMethodConfig<GreetingConfig>("", "grillhi");

            var message = config.MessageTemplate.Replace("{person}", Context.User.GetFullName());
            var converted = ConvertToBinOrHexa(message, @base);

            await ReplyAsync(converted).ConfigureAwait(false);
        }

        private string ConvertToBinOrHexa(string message, int @base)
        {
            return string.Join(" ", message.Select(o => Convert.ToString(o, @base)));
        }
    }
}
