using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Grillbot.Extensions.Discord;
using Grillbot.Services.Channelboard;
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
        private InternalStatistics InternalStatistics { get; }

        public MessageDeletedHandler(DiscordSocketClient client, ChannelStats channelStats, Logger logger, InternalStatistics internalStatistics)
        {
            Client = client;
            Statistics = channelStats;
            Logger = logger;
            InternalStatistics = internalStatistics;
        }

        private async Task OnMessageBulkDeletedAsync(IReadOnlyCollection<Cacheable<IMessage, ulong>> messages, ISocketMessageChannel channel)
        {
            foreach (var message in messages)
            {
                await OnMessageDeletedAsync(message, channel);
            }
        }

        private async Task OnMessageDeletedAsync(Cacheable<IMessage, ulong> message, ISocketMessageChannel channel)
        {
            InternalStatistics.IncrementEvent("MessageDeleted");
            if (message.HasValue && !message.Value.Author.IsUser()) return;

            if (message.Value is SocketUserMessage && channel is SocketGuildChannel socketGuildChannel)
                await Statistics.DecrementCounterAsync(socketGuildChannel);

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
