using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WatchDog_Bot.Modules;

namespace WatchDog_Bot
{
    public class CommandHandler
    {
        private DiscordSocketClient Client { get; }
        private CommandService Commands { get; }
        private IConfigurationRoot Config { get; }
        private IServiceProvider Services { get; }

        private MessageCounterModule MessageCounter { get; }
        private string CommandPrefix { get; }

        public CommandHandler(DiscordSocketClient client, CommandService commands, IConfigurationRoot config, IServiceProvider services)
        {
            Client = client;
            Commands = commands;
            Config = config;
            Services = services;

            CommandPrefix = config["CommandPrefix"];
            MessageCounter = (MessageCounterModule)Services.GetService(typeof(MessageCounterModule));

            Client.MessageReceived += OnMessageReceivedAsync;
            Client.MessageDeleted += OnMessageDeletedAsync;
            Client.MessagesBulkDeleted += BulkDeletedAsync;
        }

        private async Task BulkDeletedAsync(IReadOnlyCollection<Cacheable<IMessage, ulong>> oldMessages, ISocketMessageChannel channel)
        {
            await MessageCounter.BulkDecrement(oldMessages, channel);
        }

        private async Task OnMessageDeletedAsync(Cacheable<IMessage, ulong> oldMessage, ISocketMessageChannel channel)
        {
            await MessageCounter.Decrement(oldMessage, channel);
        }

        private async Task OnMessageReceivedAsync(SocketMessage message)
        {
            if (!(message is SocketUserMessage userMessage) || userMessage.Author.IsBot) return;
            await MessageCounter.Increment(message);

            var context = new SocketCommandContext(Client, userMessage);

            int argPos = 0;
            if(userMessage.HasStringPrefix(CommandPrefix, ref argPos))
            {
                var result = await Commands.ExecuteAsync(context, argPos, Services);

                if (!result.IsSuccess)
                    await context.Channel.SendMessageAsync($"```{result.ToString()}```");
            }
        }
    }
}
