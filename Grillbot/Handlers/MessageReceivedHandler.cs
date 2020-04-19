using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using Grillbot.Exceptions;
using Grillbot.Services.Statistics;
using Grillbot.Services;
using System.Linq;
using Microsoft.Extensions.Options;
using Grillbot.Extensions.Discord;
using Grillbot.Extensions;
using Grillbot.Services.Initiable;
using Grillbot.Modules.AutoReply;
using Grillbot.Database.Repository;
using Grillbot.Services.Channelboard;
using Grillbot.Models.Config.AppSettings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

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
        private InternalStatistics InternalStatistics { get; }
        private EmoteStats EmoteStats { get; }
        private Configuration Config { get; }
        private ILogger<MessageReceivedHandler> Logger { get; }

        public MessageReceivedHandler(DiscordSocketClient client, CommandService commands, IOptions<Configuration> config, IServiceProvider services,
            ChannelStats channelStats, AutoReplyService autoReply, EmoteChain emoteChain, InternalStatistics internalStatistics,
            EmoteStats emoteStats, ILogger<MessageReceivedHandler> logger)
        {
            Client = client;
            Commands = commands;
            Services = services;
            ChannelStats = channelStats;
            AutoReply = autoReply;
            EmoteChain = emoteChain;
            InternalStatistics = internalStatistics;
            EmoteStats = emoteStats;
            Config = config.Value;
            Logger = logger;
        }

        private async Task OnMessageReceivedAsync(SocketMessage message)
        {
            InternalStatistics.IncrementEvent("MessageReceived");

            if (!TryParseMessage(message, out SocketUserMessage userMessage)) return;

            var context = new SocketCommandContext(Client, userMessage);
            if (context.IsPrivate && !Config.IsUserBotAdmin(userMessage.Author.Id)) return;

            int argPos = 0;
            if (userMessage.HasStringPrefix(Config.CommandPrefix, ref argPos) || userMessage.HasMentionPrefix(Client.CurrentUser, ref argPos))
            {
                LogCommandAsync(userMessage, context, argPos);
                var result = await Commands.ExecuteAsync(context, userMessage.Content.Substring(argPos), Services).ConfigureAwait(false);

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

                if (context.Guild != null)
                    EmoteChain.CleanupAsync((SocketGuildChannel)context.Channel);
            }
            else
            {
                if (context.Guild != null)
                {
                    await ChannelStats.IncrementCounterAsync((SocketGuildChannel)userMessage.Channel);
                    await AutoReply.TryReplyAsync(context.Guild, userMessage).ConfigureAwait(false);
                }

                await EmoteChain.ProcessChainAsync(context).ConfigureAwait(false);
                await EmoteStats.AnylyzeMessageAndIncrementValuesAsync(context).ConfigureAwait(false);
            }
        }

        private async Task SendCommandHelp(SocketCommandContext context, int argPos)
        {
            var helpCommand = $"grillhelp {context.Message.Content.Substring(argPos)}";
            await Commands.ExecuteAsync(context, helpCommand, Services).ConfigureAwait(false);
        }

        private void LogCommandAsync(SocketUserMessage message, SocketCommandContext context, int argPos)
        {
            var commandName = GetCommand(context, argPos, out CommandMatch command);

            if (commandName != null)
            {
                var guild = context.Guild == null ? "NoGuild" : $"{context.Guild.Name} ({context.Guild.Id})";
                var channel = context.Channel == null ? "NoChannel" : $"#{context.Channel.Name} ({context.Channel.Id})";
                var args = $"{guild}, {channel}, @{context.User}, ({message.Content})";

                Logger.LogInformation("Executing {0}.\t{1}", commandName, args);
                InternalStatistics.IncrementCommand(commandName);

                using var configRepository = Services.GetService<ConfigRepository>();
                configRepository.IncrementUsageCounter(context.Guild, command.Command.Module.Group, command.Command.Name);
            }
        }

        private string GetCommand(SocketCommandContext context, int argPos, out CommandMatch command)
        {
            var substringed = context.Message.Content.Substring(argPos);
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

                command = commandInfo;
                return $"{commandInfo.Command.Module.Group} {commandInfo.Command.Name}".Trim();
            }

            command = default;
            return null;
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