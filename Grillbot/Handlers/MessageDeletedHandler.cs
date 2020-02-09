using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Grillbot.Extensions.Discord;
using Grillbot.Services.Initiable;
using Grillbot.Services.Logger;
using Grillbot.Services.Statistics;

namespace Grillbot.Handlers
{
    public class MessageDeletedHandler : IDisposable, IInitiable
    {
        private ChannelStats Statistics { get; }
        private DiscordSocketClient Client { get; }
        private Logger Logger { get; }
        private CalledEventStats CalledEventStats { get; }

        public MessageDeletedHandler(DiscordSocketClient client, ChannelStats channelStats, Logger logger, CalledEventStats calledEventStats)
        {
            Client = client;
            Statistics = channelStats;
            Logger = logger;
            CalledEventStats = calledEventStats;
        }

        private async Task OnMessageBulkDeletedAsync(IReadOnlyCollection<Cacheable<IMessage, ulong>> messages, ISocketMessageChannel channel)
        {
            foreach (var message in messages)
            {
                await OnMessageDeletedAsync(message, channel).ConfigureAwait(false);
            }
        }

        private async Task OnMessageDeletedAsync(Cacheable<IMessage, ulong> message, ISocketMessageChannel channel)
        {
            CalledEventStats.Increment("MessageDeleted");

            if (message.HasValue && !message.Value.Author.IsUser()) return;

            if (message.Value is SocketUserMessage)
                await Statistics.DecrementCounterAsync(channel).ConfigureAwait(false);

            await Logger.OnMessageDelete(message, channel).ConfigureAwait(false);
        }

        public void Dispose()
        {
            Client.MessageDeleted -= OnMessageDeletedAsync;
            Client.MessagesBulkDeleted -= OnMessageBulkDeletedAsync;
        }

        public void Init()
        {
            Client.MessageDeleted += OnMessageDeletedAsync;
            Client.MessagesBulkDeleted += OnMessageBulkDeletedAsync;
        }

        public async Task InitAsync() { }
    }
}
