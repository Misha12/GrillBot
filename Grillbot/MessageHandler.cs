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
using Grillbot.Services.EmoteStats;
using System.Linq;

namespace Grillbot
{
    public class MessageHandler : IConfigChangeable, IDisposable
    {
        private DiscordSocketClient Client { get; }
        private CommandService Commands { get; }
        private IServiceProvider Services { get; }
        private Statistics Statistics { get; }
        private AutoReplyService AutoReply { get; }
        private EmoteChain EmoteChain { get; }
        private LoggerCache LoggerCache { get; }

        private IConfiguration Config { get; set; }

        public MessageHandler(DiscordSocketClient client, CommandService commands, IConfiguration config, IServiceProvider services,
            Statistics statistics, AutoReplyService autoReply, EmoteChain emoteChain, LoggerCache loggerCache)
        {
            Client = client;
            Commands = commands;
            Services = services;
            Statistics = statistics;
            AutoReply = autoReply;
            EmoteChain = emoteChain;
            Config = config;
            LoggerCache = loggerCache;

            Client.MessageReceived += OnMessageReceivedAsync;
            Client.MessageDeleted += OnMessageDeletedAsync;
        }

        private async Task OnMessageDeletedAsync(Cacheable<IMessage, ulong> message, ISocketMessageChannel channel)
        {
            if (message.HasValue && (message.Value.Content.StartsWith(Config["CommandPrefix"]) || message.Value.Author.IsBot)) return;

            await Statistics.ChannelStats.DecrementCounterAsync(channel.Id);
            await LoggerCache.SendAttachmentToLoggerRoomAsync(message.Id);
        }

        private async Task OnMessageReceivedAsync(SocketMessage message)
        {
            var messageStopwatch = new Stopwatch();
            messageStopwatch.Start();

            try
            {
                if (!(message is SocketUserMessage userMessage) || userMessage.Author.IsBot) return;

                var commandStopwatch = new Stopwatch();
                var context = new SocketCommandContext(Client, userMessage);

                if (message.Channel is IPrivateChannel)
                {
                    var allowedAdmins = Config.GetSection($"Discord:Administrators").GetChildren().Select(o => o.Value).ToList();
                    if (!allowedAdmins.Contains(userMessage.Author.Id.ToString())) return;
                }

                int argPos = 0;
                if (userMessage.HasStringPrefix(Config["CommandPrefix"], ref argPos))
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
                    await LoggerCache.InsertMessageToCacheAsync(userMessage);
                    await Statistics.ChannelStats.IncrementCounterAsync(userMessage.Channel.Id);
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

        public void ConfigChanged(IConfiguration newConfig)
        {
            Config = newConfig;
        }

        #region IDisposable Support

        public void Dispose()
        {
            Client.MessageReceived -= OnMessageReceivedAsync;
            Client.MessageDeleted -= OnMessageDeletedAsync;
        }

        #endregion
    }
}