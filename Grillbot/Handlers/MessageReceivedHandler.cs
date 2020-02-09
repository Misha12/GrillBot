using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Grillbot.Exceptions;
using Grillbot.Services.Statistics;
using Grillbot.Services;
using System.Linq;
using Microsoft.Extensions.Options;
using Grillbot.Services.Config.Models;
using Grillbot.Extensions.Discord;
using Grillbot.Extensions;
using Grillbot.Database;
using Grillbot.Services.Initiable;
using Grillbot.Modules.AutoReply;

namespace Grillbot.Handlers
{
    public class MessageReceivedHandler : IInitiable, IDisposable
    {
        private DiscordSocketClient Client { get; }
        private CommandService Commands { get; }
        private IServiceProvider Services { get; }
        private ChannelStats ChannelStats { get; }
        private AutoReplyService AutoReply { get; }
        private EmoteChain EmoteChain { get; }
        private CalledEventStats CalledEventStats { get; }
        private Statistics Statistics { get; }
        private EmoteStats EmoteStats { get; }
        private Configuration Config { get; }

        public MessageReceivedHandler(DiscordSocketClient client, CommandService commands, IOptions<Configuration> config, IServiceProvider services,
            ChannelStats channelStats, AutoReplyService autoReply, EmoteChain emoteChain, CalledEventStats calledEventStats, Statistics statistics,
            EmoteStats emoteStats)
        {
            Client = client;
            Commands = commands;
            Services = services;
            ChannelStats = channelStats;
            AutoReply = autoReply;
            EmoteChain = emoteChain;
            CalledEventStats = calledEventStats;
            Statistics = statistics;
            EmoteStats = emoteStats;
            Config = config.Value;
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
                    await LogCommandAsync(userMessage, context, argPos).ConfigureAwait(false);

                    commandStopwatch.Start();
                    var result = await Commands.ExecuteAsync(context, userMessage.Content.Substring(argPos), Services).ConfigureAwait(false);
                    commandStopwatch.Stop();

                    if (!result.IsSuccess && result.Error != null)
                    {
                        switch (result.Error.Value)
                        {
                            case CommandError.UnknownCommand: return;
                            case CommandError.UnmetPrecondition:
                            case CommandError.ParseFailed:
                                await context.Channel.SendMessageAsync(result.ErrorReason.PreventMassTags()).ConfigureAwait(false);
                                break;
                            case CommandError.BadArgCount:
                                await SendCommandHelp(context, argPos).ConfigureAwait(false);
                                break;
                            default:
                                throw new BotException(result);
                        }
                    }

                    var command = message.Content.Split(' ')[0];
                    Statistics.LogCall(command, commandStopwatch.ElapsedMilliseconds);
                    await EmoteChain.CleanupAsync(context.Channel, true).ConfigureAwait(false);
                }
                else
                {
                    await ChannelStats.IncrementCounterAsync(userMessage.Channel).ConfigureAwait(false);
                    await AutoReply.TryReplyAsync(userMessage).ConfigureAwait(false);
                    await EmoteChain.ProcessChainAsync(context).ConfigureAwait(false);
                    await EmoteStats.AnylyzeMessageAndIncrementValuesAsync(context).ConfigureAwait(false);
                }
            }
            finally
            {
                messageStopwatch.Stop();
                Statistics.ComputeAvgReact(messageStopwatch.ElapsedMilliseconds);
            }
        }

        private async Task SendCommandHelp(SocketCommandContext context, int argPos)
        {
            var helpCommand = $"grillhelp {context.Message.Content.Substring(argPos)}";
            await Commands.ExecuteAsync(context, helpCommand, Services).ConfigureAwait(false);
        }

        private async Task LogCommandAsync(SocketUserMessage message, SocketCommandContext context, int argPos)
        {
            var substringed = message.Content.Substring(argPos);
            var searchResult = Commands.Search(context, substringed);

            if (searchResult.IsSuccess)
            {
                var commandInfo = searchResult.Commands[0];

                if (string.IsNullOrEmpty(commandInfo.Command.Name))
                {
                    var substringedSpecifyCommandSplited = substringed.Split(' ');
                    var substringedSpecifyCommand = substringedSpecifyCommandSplited.Length > 1 ? substringedSpecifyCommandSplited[1] : substringedSpecifyCommandSplited[0];
                    var validCommandInfo = searchResult.Commands.FirstOrDefault(o => o.Command.Name == substringedSpecifyCommand);

                    if (validCommandInfo.Alias != null && validCommandInfo.Command != null)
                        commandInfo = validCommandInfo;
                }

                using (var repository = new GrillBotRepository(Config))
                {
                    await repository.Log.InsertItem(commandInfo.Command.Module.Group, commandInfo.Command.Name, message.Author,
                        DateTime.Now, context.Message.Content, context.Guild, context.Channel).ConfigureAwait(false);
                }
            }
        }

        private bool TryParseMessage(SocketMessage message, out SocketUserMessage socketUserMessage)
        {
            socketUserMessage = null;

            if (!(message is SocketUserMessage userMessage))
            {
                return false;
            }

            if (!message.Author.IsUser())
                return false;

            socketUserMessage = userMessage;
            return true;
        }

        public void Dispose()
        {
            Client.MessageReceived -= OnMessageReceivedAsync;
        }

        public void Init()
        {
            Client.MessageReceived += OnMessageReceivedAsync;
        }

        public async Task InitAsync() { }
    }
}