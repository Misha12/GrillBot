using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WatchDog_Bot.Config;
using WatchDog_Bot.Extensions;

namespace WatchDog_Bot.Modules
{
    public class GreetModule : BotModuleBase
    {
        private GreetConfig Config { get; }

        public GreetModule(IConfigurationRoot config)
        {
            Config = new GreetConfig(config.GetSection("MethodsConfig:Greeting"));
        }

        [Command("hidog"), Alias("hihojkas")]
        public async Task Greet()
        {
            await Greet(Config.OutputMode);
        }

        [Command("hidog"), Alias("hihojkas")]
        public async Task Greet(string mode)
        {
            var availableModes = new[] { "text", "bin", "hexa" };

            if (!availableModes.Contains(mode)) return;
            if (!(Context.Message.Author is SocketGuildUser sender)) return;

            var message = Config.Message.Replace("{person}", sender.GetUsersFullName());

            switch (mode)
            {
                case "bin":
                    message = ConvertToBinOrHexa(message, false);
                    break;
                case "hexa":
                    message = ConvertToBinOrHexa(message, true);
                    break;
                case "text": // text
                    message = Config.Message.Replace("{person}", sender.Mention);
                    break;
            }

            var emoji = Context.Guild.Emotes.FirstOrDefault(o => o.Name == Config.AppendEmoji);
            if (!string.IsNullOrEmpty(Config.AppendEmoji) && emoji != null)
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
