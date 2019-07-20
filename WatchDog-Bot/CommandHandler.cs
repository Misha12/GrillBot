using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using WatchDog_Bot.Exceptions;
using WatchDog_Bot.Services.Statistics;

namespace WatchDog_Bot
{
    public class CommandHandler
    {
        private DiscordSocketClient Client { get; }
        private CommandService Commands { get; }
        private IServiceProvider Services { get; }
        private Statistics Statistics { get; }

        private string CommandPrefix { get; }

        public CommandHandler(DiscordSocketClient client, CommandService commands, IConfigurationRoot config, IServiceProvider services,
            Statistics statistics)
        {
            Client = client;
            Commands = commands;
            Services = services;
            Statistics = statistics;

            CommandPrefix = config["CommandPrefix"];
            Client.MessageReceived += OnMessageReceivedAsync;
        }

        private async Task OnMessageReceivedAsync(SocketMessage message)
        {
            var stopwatch = new Stopwatch();
            if (!(message is SocketUserMessage userMessage) || userMessage.Author.IsBot) return;
            var context = new SocketCommandContext(Client, userMessage);

            int argPos = 0;
            if (userMessage.HasStringPrefix(CommandPrefix, ref argPos))
            {
                stopwatch.Start();
                var result = await Commands.ExecuteAsync(context, argPos, Services);
                stopwatch.Stop();

                if (!result.IsSuccess && result.Error != null)
                {
                    switch (result.Error.Value)
                    {
                        case CommandError.UnknownCommand: return;
                        case CommandError.BadArgCount:
                            await context.Channel.SendMessageAsync($"Nedostatečný počet parametrů.");
                            break;
                        default:
                            throw new BotException(result);
                    }
                }

                var command = message.Content.Split(' ')[0];
                Statistics.LogCall(command, stopwatch.ElapsedMilliseconds);
            }
        }
    }
}
