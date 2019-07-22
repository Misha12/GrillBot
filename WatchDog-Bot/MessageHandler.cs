using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using WatchDog_Bot.Exceptions;
using WatchDog_Bot.Modules;
using WatchDog_Bot.Services;
using WatchDog_Bot.Services.Statistics;

namespace WatchDog_Bot
{
    public class MessageHandler
    {
        private DiscordSocketClient Client { get; }
        private CommandService Commands { get; }
        private IServiceProvider Services { get; }
        private Statistics Statistics { get; }
        private AutoReplyModule AutoReply { get; }
        private EmoteChain EmoteChain { get; }

        private string CommandPrefix { get; }

        public MessageHandler(DiscordSocketClient client, CommandService commands, IConfigurationRoot config, IServiceProvider services,
            Statistics statistics, AutoReplyModule autoReply, EmoteChain emoteChain)
        {
            Client = client;
            Commands = commands;
            Services = services;
            Statistics = statistics;
            AutoReply = autoReply;
            EmoteChain = emoteChain;

            CommandPrefix = config["CommandPrefix"];

            Client.MessageReceived += OnMessageReceivedAsync;
            Client.MessageDeleted += OnMessageDeletedAsync;
        }

        private async Task OnMessageDeletedAsync(Cacheable<IMessage, ulong> message, ISocketMessageChannel channel)
        {
            if (message.HasValue && (message.Value.Content.StartsWith(CommandPrefix) || message.Value.Author.IsBot)) return;

            Statistics.DecrementChannelCounter(channel.Id);
        }

        private async Task OnMessageReceivedAsync(SocketMessage message)
        {
            if (!(message is SocketUserMessage userMessage) || userMessage.Author.IsBot) return;

            var messageStopwatch = new Stopwatch();
            messageStopwatch.Start();

            try
            {
                var commandStopwatch = new Stopwatch();
                var context = new SocketCommandContext(Client, userMessage);

                int argPos = 0;
                if (userMessage.HasStringPrefix(CommandPrefix, ref argPos))
                {
                    commandStopwatch.Start();
                    var result = await Commands.ExecuteAsync(context, argPos, Services);
                    commandStopwatch.Stop();

                    if (!result.IsSuccess && result.Error != null)
                    {
                        switch (result.Error.Value)
                        {
                            case CommandError.UnknownCommand: return;
                            case CommandError.UnmetPrecondition:
                                await context.Channel.SendMessageAsync($"Na tento příkaz nemáš dostatečná práva.");
                                break;
                            case CommandError.BadArgCount:
                                await context.Channel.SendMessageAsync($"Nedostatečný počet parametrů.");
                                break;
                            default:
                                throw new BotException(result);
                        }
                    }

                    var command = message.Content.Split(' ')[0];

                    Statistics.LogCall(command, commandStopwatch.ElapsedMilliseconds);
                    EmoteChain.Cleanup(context.Channel);
                }
                else
                {
                    Statistics.IncrementChannelCounter(userMessage.Channel.Id);
                    await AutoReply.TryReply(userMessage);
                    await EmoteChain.ProcessChain(context);
                }
            }
            finally
            {
                messageStopwatch.Stop();
                Statistics.ComputeAvgReact(messageStopwatch.ElapsedMilliseconds);
            }
        }
    }
}