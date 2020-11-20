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
        private AutoReplyService AutoReply { get; }
        private EmoteChain EmoteChain { get; }
        private InternalStatistics InternalStatistics { get; }
        private UserService UserService { get; }
        private BotStatusService BotStatus { get; }
        private ConfigurationService ConfigurationService { get; }

        public MessageReceivedHandler(DiscordSocketClient client, CommandService commands, IServiceProvider services,
            AutoReplyService autoReply, EmoteChain emoteChain, InternalStatistics internalStatistics, UserService userService,
            BotStatusService botStatus, ConfigurationService configurationService)
        {
            Client = client;
            Commands = commands;
            Services = services;
            AutoReply = autoReply;
            EmoteChain = emoteChain;
            InternalStatistics = internalStatistics;
            UserService = userService;
            BotStatus = botStatus;
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
                BotStatus.RunningCommands.Add(message);
                await Commands.ExecuteAsync(context, userMessage.Content[argPos..], Services).ConfigureAwait(false);

                if (context.Guild != null)
                    EmoteChain.CleanupAsync((SocketGuildChannel)context.Channel);
            }
            else
            {
                if (context.Guild != null)
                {
                    IncrementPoints(context.Guild, message);
                    UserService.IncrementMessage(context.User as SocketGuildUser, context.Guild, context.Channel as SocketGuildChannel);
                    await AutoReply.TryReplyAsync(context.Guild, userMessage).ConfigureAwait(false);
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

        private void IncrementPoints(SocketGuild guild, SocketMessage message)
        {
            using var scope = Services.CreateScope();
            using var pointsService = scope.ServiceProvider.GetService<PointsService>();

            pointsService.IncrementPoints(guild, message);
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