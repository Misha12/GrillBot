using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using Grillbot.Services.Statistics;
using Grillbot.Services;
using Grillbot.Extensions.Discord;
using Grillbot.Services.Initiable;
using Grillbot.Modules.AutoReply;
using Microsoft.Extensions.DependencyInjection;
using Grillbot.Services.UserManagement;
using Microsoft.Extensions.Configuration;

namespace Grillbot.Handlers
{
    public class MessageReceivedHandler : IInitiable, IDisposable, IHandler
    {
        private DiscordSocketClient Client { get; }
        private CommandService Commands { get; }
        private IServiceProvider Services { get; }
        private EmoteChain EmoteChain { get; }
        private InternalStatistics InternalStatistics { get; }
        private IConfiguration Configuration { get; }
        private BotState BotState { get; }

        public MessageReceivedHandler(DiscordSocketClient client, CommandService commands, IServiceProvider services,
            EmoteChain emoteChain, InternalStatistics internalStatistics, IConfiguration configuration, BotState botState)
        {
            Client = client;
            Commands = commands;
            Services = services;
            EmoteChain = emoteChain;
            InternalStatistics = internalStatistics;
            Configuration = configuration;
            BotState = botState;
        }

        private async Task OnMessageReceivedAsync(SocketMessage message)
        {
            if (!TryParseMessage(message, out SocketUserMessage userMessage)) return;
            InternalStatistics.IncrementEvent("MessageReceived");

            var context = new SocketCommandContext(Client, userMessage);
            if (context.IsPrivate) return;

            int argPos = 0;
            if (IsCommand(userMessage, ref argPos))
            {
                BotState.RunningCommands.Add(message);
                await Commands.ExecuteAsync(context, userMessage.Content[argPos..], Services).ConfigureAwait(false);

                if (context.Guild != null)
                    EmoteChain.Cleanup((SocketGuildChannel)context.Channel);
            }
            else
            {
                using var scope = Services.CreateScope();

                if (context.Guild != null)
                {
                    await scope.ServiceProvider.GetService<PointsService>().IncrementPointsAsync(context.Guild, message);
                    await scope.ServiceProvider.GetService<UserMessagesService>().IncrementMessageStats(context.Guild, context.User, context.Channel);
                    await scope.ServiceProvider.GetService<AutoReplyService>().TryReplyAsync(context.Guild, userMessage);
                }

                await EmoteChain.ProcessChainAsync(context).ConfigureAwait(false);
                await scope.ServiceProvider.GetService<EmoteStats>().AnylyzeMessageAndIncrementValuesAsync(context);
            }
        }

        private bool TryParseMessage(SocketMessage message, out SocketUserMessage socketUserMessage)
        {
            socketUserMessage = null;

            if (message is not SocketUserMessage userMessage || !message.Author.IsUser())
                return false;

            socketUserMessage = userMessage;
            return true;
        }

        private bool IsCommand(SocketUserMessage message, ref int argPos)
        {
            if (message.HasMentionPrefix(Client.CurrentUser, ref argPos))
                return true;

            var prefix = Configuration["CommandPrefix"];
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