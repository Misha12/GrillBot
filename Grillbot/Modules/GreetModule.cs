using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grillbot.Services;

namespace Grillbot.Modules
{
    [Name("Pozdrav bota")]
    public class GreetModule : BotModuleBase
    {
        private IConfiguration Config { get; }

        public GreetModule(IConfiguration config)
        {
            Config = config.GetSection("MethodsConfig:Greeting");
        }

        [Command("grillhi"), Alias("hojkashi")]
        [DisabledCheck(RoleGroupName = "Greeting")]
        [RequireRoleOrAdmin(RoleGroupName = "Greeting")]
        public async Task GreetAsync()
        {
            await GreetAsync(Config["OutputMode"]);
        }

        [Command("grillhi"), Alias("hojkashi")]
        [DisabledCheck(RoleGroupName = "Greeting")]
        [Remarks("Možné formáty odpovědi jsou 'text', 'bin', nebo 'hex'.")]
        [RequireRoleOrAdmin(RoleGroupName = "Greeting")]
        public async Task GreetAsync(string mode)
        {
            var availableModes = new[] { "text", "bin", "hex" };

            if (!availableModes.Contains(mode)) return;
            if (!(Context.Message.Author is SocketGuildUser sender)) return;
            var messageTemplate = Config["Message"];

            var message = messageTemplate.Replace("{person}", GetUsersFullName(sender));

            switch (mode)
            {
                case "bin":
                    message = ConvertToBinOrHexa(message, 2);
                    break;
                case "hex":
                    message = ConvertToBinOrHexa(message, 16);
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

        [Command("grillhi"), Alias("hojkashi")]
        [DisabledCheck(RoleGroupName = "Greeting")]
        [Remarks("Možné základy soustav odpovědi jsou 2, 8, 10, nebo 16.")]
        [RequireRoleOrAdmin(RoleGroupName = "Greeting")]
        public async Task GreetAsync(int @base)
        {
            var supportedBases = new[] { 2, 8, 10, 16 };

            if (!supportedBases.Contains(@base)) return;
            if (!(Context.Message.Author is SocketGuildUser sender)) return;

            var messageTemplate = Config["Message"];
            var message = messageTemplate.Replace("{person}", GetUsersFullName(sender));
            var converted = ConvertToBinOrHexa(message, @base);

            var emoji = Context.Guild.Emotes.FirstOrDefault(o => o.Name == Config["AppendEmoji"]);
            if (emoji != null)
                await ReplyAsync($"{converted} <:{emoji.Name}:{emoji.Id}>");
            else
                await ReplyAsync(converted);
        }

        private string ConvertToBinOrHexa(string message, int @base)
        {
            return string.Join(" ", message.Select(o => Convert.ToString(o, @base)));
        }
    }
}
