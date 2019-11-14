using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Grillbot.Exceptions;
using Grillbot.Modules;
using Grillbot.Services.Statistics;
using Grillbot.Services;
using System.Linq;
using Grillbot.Services.Config;
using Microsoft.Extensions.Options;
using Grillbot.Services.Config.Models;

namespace Grillbot.Handlers
{
    public class MessageReceivedHandler : IConfigChangeable, IHandle
    {
        private DiscordSocketClient Client { get; }
        private CommandService Commands { get; }
        private IServiceProvider Services { get; }
        private Statistics Statistics { get; }
        private AutoReplyService AutoReply { get; }
        private EmoteChain EmoteChain { get; }
        private CalledEventStats CalledEventStats { get; }

        private Configuration Config { get; set; }

        public MessageReceivedHandler(DiscordSocketClient client, CommandService commands, IOptions<Configuration> config, IServiceProvider services,
            Statistics statistics, AutoReplyService autoReply, EmoteChain emoteChain, CalledEventStats calledEventStats)
        {
            Client = client;
            Commands = commands;
            Services = services;
            Statistics = statistics;
            AutoReply = autoReply;
            EmoteChain = emoteChain;
            CalledEventStats = calledEventStats;

            ConfigChanged(config.Value);

            Client.MessageReceived += OnMessageReceivedAsync;
        }

        private async Task OnMessageReceivedAsync(SocketMessage message)
        {
            CalledEventStats.Increment("MessageReceived");

            var messageStopwatch = new Stopwatch();
            messageStopwatch.Start();

            try
            {
                if (!TryParseMessage(message, out SocketUserMessage userMessage)) return;

                var commandStopwatch = new Stopwatch();
                var context = new SocketCommandContext(Client, userMessage);

                if (message.Channel is IPrivateChannel && !Config.IsUserBotAdmin(userMessage.Author.Id)) return;

                int argPos = 0;
                if (userMessage.HasStringPrefix(Config.CommandPrefix, ref argPos) || userMessage.HasMentionPrefix(Client.CurrentUser, ref argPos))
                {
                    commandStopwatch.Start();
                    var result = await Commands.ExecuteAsync(context, userMessage.Content.Substring(argPos), Services);
                    commandStopwatch.Stop();

                    if (!result.IsSuccess && result.Error != null)
                    {
                        switch (result.Error.Value)
                        {
                            case CommandError.UnknownCommand: return;
                            case CommandError.UnmetPrecondition:
                            case CommandError.ParseFailed:
                                await context.Channel.SendMessageAsync(result.ErrorReason);
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
                    await EmoteChain.CleanupAsync(context.Channel, true);
                }
                else
                {
                    await Statistics.ChannelStats.IncrementCounterAsync(userMessage.Channel);
                    await AutoReply.TryReplyAsync(userMessage);
                    await EmoteChain.ProcessChainAsync(context);
                    await Statistics.EmoteStats.AnylyzeMessageAndIncrementValuesAsync(context);
                }
            }
            finally
            {
                messageStopwatch.Stop();
                Statistics.ComputeAvgReact(messageStopwatch.ElapsedMilliseconds);
            }
        }

        private bool TryParseMessage(SocketMessage message, out SocketUserMessage socketUserMessage)
        {
            socketUserMessage = null;

            if(!(message is SocketUserMessage userMessage))
            {
                return false;
            }

            if (message.Author.IsBot || message.Author.IsWebhook)
                return false;

            socketUserMessage = userMessage;
            return true;
        }

        public void ConfigChanged(Configuration newConfig)
        {
            Config = newConfig;
        }

        public void Dispose()
        {
            Client.MessageReceived -= OnMessageReceivedAsync;
        }
    }
}