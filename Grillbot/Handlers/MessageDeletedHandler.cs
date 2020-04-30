using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Grillbot.Extensions.Discord;
using Grillbot.Services;
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
        private PaginationService PaginationService { get; }

        public MessageDeletedHandler(DiscordSocketClient client, ChannelStats channelStats, Logger logger, InternalStatistics internalStatistics,
            PaginationService paginationService)
        {
            Client = client;
            Statistics = channelStats;
            Logger = logger;
            InternalStatistics = internalStatistics;
            PaginationService = paginationService;
        }

        private async Task OnMessageDeletedAsync(Cacheable<IMessage, ulong> message, ISocketMessageChannel channel)
        {
            InternalStatistics.IncrementEvent("MessageDeleted");
            if (message.HasValue && !message.Value.Author.IsUser()) return;

            if (message.Value is SocketUserMessage && channel is SocketGuildChannel socketGuildChannel)
                await Statistics.DecrementCounterAsync(socketGuildChannel);

            await Logger.OnMessageDelete(message, channel).ConfigureAwait(false);
            PaginationService.DeleteEmbed(message.Id);
        }

        public void Dispose()
        {
            Client.MessageDeleted -= OnMessageDeletedAsync;
        }

        public void Init()
        {
            Client.MessageDeleted += OnMessageDeletedAsync;
        }

        public async Task InitAsync() { }
    }
}
