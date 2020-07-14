using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using Grillbot.Services.Statistics;
using Grillbot.Services;
using Microsoft.Extensions.Options;
using Grillbot.Extensions.Discord;
using Grillbot.Services.Initiable;
using Grillbot.Modules.AutoReply;
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
        private BotStatusService BotStatus { get; }

        public MessageReceivedHandler(DiscordSocketClient client, CommandService commands, IOptions<Configuration> config, IServiceProvider services,
            AutoReplyService autoReply, EmoteChain emoteChain, InternalStatistics internalStatistics, EmoteStats emoteStats, UserService userService,
            BotStatusService botStatus)
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
            BotStatus = botStatus;
        }

        private async Task OnMessageReceivedAsync(SocketMessage message)
        {
            InternalStatistics.IncrementEvent("MessageReceived");

            if (!TryParseMessage(message, out SocketUserMessage userMessage)) return;

            var context = new SocketCommandContext(Client, userMessage);
            if (context.IsPrivate) return;

            int argPos = 0;
            if (userMessage.HasStringPrefix(Config.CommandPrefix, ref argPos) || userMessage.HasMentionPrefix(Client.CurrentUser, ref argPos))
            {
                BotStatus.RunningCommands.Add(message);
                await Commands.ExecuteAsync(context, userMessage.Content.Substring(argPos), Services).ConfigureAwait(false);

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
                EmoteStats.AnylyzeMessageAndIncrementValues(context);
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