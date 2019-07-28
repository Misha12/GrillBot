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

namespace Grillbot
{
    public class MessageHandler : IConfigChangeable, IDisposable
    {
        private DiscordSocketClient Client { get; }
        private CommandService Commands { get; }
        private IServiceProvider Services { get; }
        private Statistics Statistics { get; }
        private AutoReplyModule AutoReply { get; }
        private EmoteChain EmoteChain { get; }

        private IConfiguration Config { get; set; }

        public MessageHandler(DiscordSocketClient client, CommandService commands, IConfiguration config, IServiceProvider services,
            Statistics statistics, AutoReplyModule autoReply, EmoteChain emoteChain)
        {
            Client = client;
            Commands = commands;
            Services = services;
            Statistics = statistics;
            AutoReply = autoReply;
            EmoteChain = emoteChain;
            Config = config;

            Client.MessageReceived += OnMessageReceivedAsync;
            Client.MessageDeleted += OnMessageDeletedAsync;
            Client.UserJoined += OnUserJoinedOnServerAsync;
        }

        private async Task OnUserJoinedOnServerAsync(SocketGuildUser user)
        {
            var message = Config["Discord:UserJoinedMessage"];

            if (!string.IsNullOrEmpty(message))
                await user.SendMessageAsync(message);
        }

        private async Task OnMessageDeletedAsync(Cacheable<IMessage, ulong> message, ISocketMessageChannel channel)
        {
            if (message.HasValue && (message.Value.Content.StartsWith(Config["CommandPrefix"]) || message.Value.Author.IsBot)) return;

            await Statistics.ChannelStats.DecrementCounter(channel.Id);
        }

        private async Task OnMessageReceivedAsync(SocketMessage message)
        {
            var messageStopwatch = new Stopwatch();
            messageStopwatch.Start();

            try
            {
                if (!(message is SocketUserMessage userMessage) || userMessage.Author.IsBot) return;
                if (message.Channel is IPrivateChannel) return;

                var commandStopwatch = new Stopwatch();
                var context = new SocketCommandContext(Client, userMessage);

                int argPos = 0;
                if (userMessage.HasStringPrefix(Config["CommandPrefix"], ref argPos))
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
                    await EmoteChain.Cleanup(context.Channel, true);
                }
                else
                {
                    await Statistics.ChannelStats.IncrementCounter(userMessage.Channel.Id);
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

        public void ConfigChanged(IConfiguration newConfig)
        {
            Config = newConfig;
        }

        #region IDisposable Support

        protected virtual void Dispose(bool disposing)
        {
            if(disposing)
            {
                Client.MessageReceived -= OnMessageReceivedAsync;
                Client.MessageDeleted -= OnMessageDeletedAsync;
                Client.UserJoined -= OnUserJoinedOnServerAsync;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}