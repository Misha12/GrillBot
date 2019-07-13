using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace WatchDog_Bot
{
    public class CommandHandler
    {
        private DiscordSocketClient Client { get; }
        private CommandService Commands { get; }
        private IConfigurationRoot Config { get; }
        private IServiceProvider Services { get; }

        private string CommandPrefix { get; }

        public CommandHandler(DiscordSocketClient client, CommandService commands, IConfigurationRoot config, IServiceProvider services)
        {
            Client = client;
            Commands = commands;
            Config = config;
            Services = services;

            CommandPrefix = config["CommandPrefix"];
            Client.MessageReceived += OnMessageReceivedAsync;
        }

        private async Task OnMessageReceivedAsync(SocketMessage message)
        {
            if (!(message is SocketUserMessage userMessage) || userMessage.Author.IsBot) return;
            var context = new SocketCommandContext(Client, userMessage);

            int argPos = 0;
            if (userMessage.HasStringPrefix(CommandPrefix, ref argPos))
            {
                var result = await Commands.ExecuteAsync(context, argPos, Services);

                if (!result.IsSuccess)
                    await context.Channel.SendMessageAsync($"```{result.ToString()}```");
            }
        }
    }
}
