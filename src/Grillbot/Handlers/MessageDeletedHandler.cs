using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Grillbot.Extensions.Discord;
using Grillbot.Services;
using Grillbot.Services.Initiable;
using Grillbot.Services.Logger;
using Grillbot.Services.MessageCache;
using Grillbot.Services.Statistics;
using Grillbot.Services.UserManagement;
using Microsoft.Extensions.DependencyInjection;

namespace Grillbot.Handlers
{
    public class MessageDeletedHandler : IDisposable, IInitiable
    {
        private DiscordSocketClient Client { get; }
        private Logger Logger { get; }
        private InternalStatistics InternalStatistics { get; }
        private PaginationService PaginationService { get; }
        private IMessageCache MessageCache { get; }
        private IServiceProvider Provider { get; }

        public MessageDeletedHandler(DiscordSocketClient client, Logger logger, InternalStatistics internalStatistics,
            PaginationService paginationService, IMessageCache messageCache, IServiceProvider provider)
        {
            Client = client;
            Logger = logger;
            InternalStatistics = internalStatistics;
            PaginationService = paginationService;
            MessageCache = messageCache;
            Provider = provider;
        }

        private async Task OnMessageDeletedAsync(Cacheable<IMessage, ulong> message, ISocketMessageChannel channel)
        {
            InternalStatistics.IncrementEvent("MessageDeleted");
            if (channel is IPrivateChannel || (message.HasValue && !message.Value.Author.IsUser())) return;

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

            using var scope = Provider.CreateScope();

            if (user != null)
                await scope.ServiceProvider.GetService<UserMessagesService>().DecrementMessageStats(user.Guild, user, channel);

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
