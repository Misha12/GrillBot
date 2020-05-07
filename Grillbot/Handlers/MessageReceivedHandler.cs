using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using Grillbot.Exceptions;
using Grillbot.Services.Statistics;
using Grillbot.Services;
using Microsoft.Extensions.Options;
using Grillbot.Extensions.Discord;
using Grillbot.Extensions;
using Grillbot.Services.Initiable;
using Grillbot.Modules.AutoReply;
using Grillbot.Services.Channelboard;
using Grillbot.Models.Config.AppSettings;
using Grillbot.Services.UserManagement;

namespace Grillbot.Handlers
{
    public class MessageReceivedHandler : IInitiable, IDisposable
    {
        private DiscordSocketClient Client { get; }
        private CommandService Commands { get; }
        private IServiceProvider Services { get; }
        private AutoReplyService AutoReply { get; }
        private EmoteChain EmoteChain { get; }
        private InternalStatistics InternalStatistics { get; }
        private EmoteStats EmoteStats { get; }
        private Configuration Config { get; }
        private UserService UserService { get; }

        public MessageReceivedHandler(DiscordSocketClient client, CommandService commands, IOptions<Configuration> config, IServiceProvider services,
            AutoReplyService autoReply, EmoteChain emoteChain, InternalStatistics internalStatistics, EmoteStats emoteStats, UserService userService)
        {
            Client = client;
            Commands = commands;
            Services = services;
            AutoReply = autoReply;
            EmoteChain = emoteChain;
            InternalStatistics = internalStatistics;
            EmoteStats = emoteStats;
            Config = config.Value;
            UserService = userService;
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
                    UserService.IncrementMessage(context.User as SocketGuildUser, context.Guild, context.Channel as SocketGuildChannel);
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