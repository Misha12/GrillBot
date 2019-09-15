using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Grillbot.Services.Config;
using Grillbot.Services.Config.Models;
using Grillbot.Services.Logger;
using Grillbot.Services.Statistics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Grillbot.Handlers
{
    public class MessageDeletedHandler : IConfigChangeable, IHandle
    {
        private Statistics Statistics { get; }
        private Configuration Config { get; set; }
        private DiscordSocketClient Client { get; }
        private Logger Logger { get; }

        public MessageDeletedHandler(DiscordSocketClient client, Statistics statistics, IOptions<Configuration> config, Logger logger)
        {
            Client = client;
            Statistics = statistics;
            Logger = logger;

            //ConfigChanged(config.Value); TODO

            Client.MessageDeleted += OnMessageDeletedAsync;
            Client.MessagesBulkDeleted += OnMessageBulkDeletedAsync;
        }

        private async Task OnMessageBulkDeletedAsync(IReadOnlyCollection<Cacheable<IMessage, ulong>> messages, ISocketMessageChannel channel)
        {
            foreach(var message in messages)
            {
                await OnMessageDeletedAsync(message, channel);
            }
        }

        private async Task OnMessageDeletedAsync(Cacheable<IMessage, ulong> message, ISocketMessageChannel channel)
        {
            if(message.HasValue)
            {
                if(message.Value is SocketUserMessage userMessage)
                {
                    int argPos = 0;
                    if (userMessage.HasStringPrefix(Config.CommandPrefix, ref argPos)) return;
                }

                if (message.Value.Author.IsBot || message.Value.Author.IsWebhook) return;
            }

            await Statistics.ChannelStats.DecrementCounterAsync(channel);
            await Logger.OnMessageDelete(message, channel);
        }

        public void ConfigChanged(IConfiguration newConfig)
        {
            //TODO
            //Config = newConfig;
        }

        public void Dispose()
        {
            Client.MessageDeleted -= OnMessageDeletedAsync;
            Client.MessagesBulkDeleted -= OnMessageBulkDeletedAsync;
        }
    }
}
