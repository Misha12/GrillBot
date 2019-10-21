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
        private CalledEventStats CalledEventStats { get; }

        public MessageDeletedHandler(DiscordSocketClient client, Statistics statistics, IOptions<Configuration> config, Logger logger,
            CalledEventStats calledEventStats)
        {
            Client = client;
            Statistics = statistics;
            Logger = logger;
            CalledEventStats = calledEventStats;

            ConfigChanged(config.Value);

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
            CalledEventStats.Increment("MessageDeleted");
            
            if (message.HasValue && (message.Value.Author.IsBot || message.Value.Author.IsWebhook)) return;

            if (message.Value is SocketUserMessage)
                await Statistics.ChannelStats.DecrementCounterAsync(channel);
            
            await Logger.OnMessageDelete(message, channel);
        }

        public void ConfigChanged(Configuration newConfig)
        {
            Config = newConfig;
        }

        public void Dispose()
        {
            Client.MessageDeleted -= OnMessageDeletedAsync;
            Client.MessagesBulkDeleted -= OnMessageBulkDeletedAsync;
        }
    }
}
