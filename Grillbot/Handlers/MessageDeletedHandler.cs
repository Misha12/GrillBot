using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Grillbot.Extensions.Discord;
using Grillbot.Services.Logger;
using Grillbot.Services.Statistics;

namespace Grillbot.Handlers
{
    public class MessageDeletedHandler : IHandle
    {
        private Statistics Statistics { get; }
        private DiscordSocketClient Client { get; }
        private Logger Logger { get; }
        private CalledEventStats CalledEventStats { get; }

        public MessageDeletedHandler(DiscordSocketClient client, Statistics statistics, Logger logger, CalledEventStats calledEventStats)
        {
            Client = client;
            Statistics = statistics;
            Logger = logger;
            CalledEventStats = calledEventStats;

            Client.MessageDeleted += OnMessageDeletedAsync;
            Client.MessagesBulkDeleted += OnMessageBulkDeletedAsync;
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
                await Statistics.ChannelStats.DecrementCounterAsync(channel).ConfigureAwait(false);

            await Logger.OnMessageDelete(message, channel).ConfigureAwait(false);
        }

        public void Dispose()
        {
            Client.MessageDeleted -= OnMessageDeletedAsync;
            Client.MessagesBulkDeleted -= OnMessageBulkDeletedAsync;
        }
    }
}
