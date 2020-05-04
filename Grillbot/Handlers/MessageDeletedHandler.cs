using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Grillbot.Extensions.Discord;
using Grillbot.Services;
using Grillbot.Services.Channelboard;
using Grillbot.Services.Initiable;
using Grillbot.Services.Logger;
using Grillbot.Services.MessageCache;
using Grillbot.Services.Statistics;
using Grillbot.Services.UserManagement;

namespace Grillbot.Handlers
{
    public class MessageDeletedHandler : IDisposable, IInitiable
    {
        private ChannelStats ChannelStats { get; }
        private DiscordSocketClient Client { get; }
        private Logger Logger { get; }
        private InternalStatistics InternalStatistics { get; }
        private PaginationService PaginationService { get; }
        private IMessageCache MessageCache { get; }
        private UserService UserService { get; }

        public MessageDeletedHandler(DiscordSocketClient client, ChannelStats channelStats, Logger logger, InternalStatistics internalStatistics,
            PaginationService paginationService, IMessageCache messageCache, UserService userService)
        {
            Client = client;
            ChannelStats = channelStats;
            Logger = logger;
            InternalStatistics = internalStatistics;
            PaginationService = paginationService;
            MessageCache = messageCache;
            UserService = userService;
        }

        private async Task OnMessageDeletedAsync(Cacheable<IMessage, ulong> message, ISocketMessageChannel channel)
        {
            InternalStatistics.IncrementEvent("MessageDeleted");
            if (message.HasValue && !message.Value.Author.IsUser()) return;

            var socketGuildChannel = channel as SocketGuildChannel;

            if (message.Value is SocketUserMessage && socketGuildChannel != null)
                await ChannelStats.DecrementCounterAsync(socketGuildChannel);

            SocketGuildUser user = null;
            if (message.HasValue && message.Value.Author is SocketGuildUser guildUser)
            {
                user = guildUser;
            }
            else if (MessageCache.Exists(message.Id))
            {
                var author = MessageCache.Get(message.Id).Author;
                user = author is SocketGuildUser socketGuildUser ? socketGuildUser : null;
            }

            if (user != null && socketGuildChannel != null)
                UserService.DecrementMessage(user, user.Guild, socketGuildChannel);

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
