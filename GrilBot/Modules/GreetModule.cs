using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrilBot.Services;

namespace GrilBot.Modules
{
    [Name("Pozdrav bota")]
    public class GreetModule : BotModuleBase
    {
        private IConfiguration Config { get; }

        public GreetModule(IConfigurationRoot config)
        {
            Config = config.GetSection("MethodsConfig:Greeting");
        }

        [Command("hidog")]
        [RequireRole(RoleGroupName = "Greeting")]
        public async Task Greet()
        {
            await Greet(Config["OutputMode"]);
        }

        [Command("hidog")]
        [Remarks("Možné formáty odpověi jsou 'text', 'bin', nebo 'hex'.")]
        [RequireRole(RoleGroupName = "Greeting")]
        public async Task Greet(string mode)
        {
            var availableModes = new[] { "text", "bin", "hex" };

            if (!availableModes.Contains(mode)) return;
            if (!(Context.Message.Author is SocketGuildUser sender)) return;
            var messageTemplate = Config["Message"];

            var message = messageTemplate.Replace("{person}", GetUsersFullName(sender));

            switch (mode)
            {
                case "bin":
                    message = ConvertToBinOrHexa(message, false);
                    break;
                case "hex":
                    message = ConvertToBinOrHexa(message, true);
                    break;
                case "text": // text
                    message = messageTemplate.Replace("{person}", sender.Mention);
                    break;
            }

            var emoji = Context.Guild.Emotes.FirstOrDefault(o => o.Name == Config["AppendEmoji"]);
            if (emoji != null)
                await ReplyAsync($"{message} <:{emoji.Name}:{emoji.Id}>");
            else
                await ReplyAsync(message);
        }

        private string ConvertToBinOrHexa(string message, bool useHexa)
        {
            var values = new List<string>(message.Length * 8); // 8 bits per char.
            int @base = useHexa ? 16 : 2;

            foreach(int charCode in message)
            {
                values.Add(Convert.ToString(charCode, @base));
            }

            return string.Join(" ", values);
        }
    }
}
