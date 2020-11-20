using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using Grillbot.Services.Statistics;
using Grillbot.Services;
using Grillbot.Extensions.Discord;
using Grillbot.Services.Initiable;
using Grillbot.Modules.AutoReply;
using Grillbot.Services.UserManagement;
using Microsoft.Extensions.DependencyInjection;
using Grillbot.Services.Config;
using Grillbot.Enums;

namespace Grillbot.Handlers
{
    public class MessageReceivedHandler : IInitiable, IDisposable
    {
        private DiscordSocketClient Client { get; }
        private CommandService Commands { get; }
        private IServiceProvider Services { get; }
        private EmoteChain EmoteChain { get; }
        private InternalStatistics InternalStatistics { get; }
        private UserService UserService { get; }
        private ConfigurationService ConfigurationService { get; }

        public MessageReceivedHandler(DiscordSocketClient client, CommandService commands, IServiceProvider services,
            EmoteChain emoteChain, InternalStatistics internalStatistics, UserService userService,
            ConfigurationService configurationService)
        {
            Client = client;
            Commands = commands;
            Services = services;
            EmoteChain = emoteChain;
            InternalStatistics = internalStatistics;
            UserService = userService;
            ConfigurationService = configurationService;
        }

        private async Task OnMessageReceivedAsync(SocketMessage message)
        {
            using var scope = Services.CreateScope();

            InternalStatistics.IncrementEvent("MessageReceived");

            if (!TryParseMessage(message, out SocketUserMessage userMessage)) return;

            var context = new SocketCommandContext(Client, userMessage);
            if (context.IsPrivate) return;

            int argPos = 0;
            if (IsCommand(userMessage, ref argPos))
            {
                scope.ServiceProvider.GetService<BotState>().RunningCommands.Add(message);
                await Commands.ExecuteAsync(context, userMessage.Content[argPos..], Services).ConfigureAwait(false);

                if (context.Guild != null)
                    EmoteChain.CleanupAsync((SocketGuildChannel)context.Channel);
            }
            else
            {
                if (context.Guild != null)
                {
                    scope.ServiceProvider.GetService<PointsService>().IncrementPoints(context.Guild, message);
                    UserService.IncrementMessage(context.User as SocketGuildUser, context.Guild, context.Channel as SocketGuildChannel);
                    await scope.ServiceProvider.GetService<AutoReplyService>().TryReplyAsync(context.Guild, userMessage);
                }

                await EmoteChain.ProcessChainAsync(context).ConfigureAwait(false);
                await scope.ServiceProvider.GetService<EmoteStats>().AnylyzeMessageAndIncrementValuesAsync(context);
            }
        }

        private bool TryParseMessage(SocketMessage message, out SocketUserMessage socketUserMessage)
        {
            socketUserMessage = null;

            if (message is not SocketUserMessage userMessage)
                return false;

            if (!message.Author.IsUser())
                return false;

            socketUserMessage = userMessage;
            return true;
        }

        private bool IsCommand(SocketUserMessage message, ref int argPos)
        {
            if (message.HasMentionPrefix(Client.CurrentUser, ref argPos))
                return true;

            var prefix = ConfigurationService.GetValue(GlobalConfigItems.CommandPrefix);
            if (string.IsNullOrEmpty(prefix))
                prefix = "$";

            return message.Content.Length > prefix.Length && message.HasStringPrefix(prefix, ref argPos);
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