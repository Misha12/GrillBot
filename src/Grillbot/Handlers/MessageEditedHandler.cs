using Discord;
using Discord.WebSocket;
using Grillbot.Extensions.Discord;
using Grillbot.Services.Audit;
using Grillbot.Services.Initiable;
using Grillbot.Services.Statistics;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Grillbot.Handlers
{
    public class MessageEditedHandler : IDisposable, IInitiable
    {
        private DiscordSocketClient Client { get; }
        private InternalStatistics InternalStatistics { get; }
        private IServiceProvider Provider { get; }

        public MessageEditedHandler(DiscordSocketClient client, InternalStatistics internalStatistics, IServiceProvider provider)
        {
            Client = client;
            InternalStatistics = internalStatistics;
            Provider = provider;
        }

        private async Task OnMessageUpdatedAsync(Cacheable<IMessage, ulong> messageBefore, SocketMessage messageAfter, ISocketMessageChannel channel)
        {
            InternalStatistics.IncrementEvent("MessageUpdated");

            if (!messageAfter.Author.IsUser() || channel is IPrivateChannel) return;
            if (channel is SocketGuildChannel guildChannel)
            {
                using var scope = Provider.CreateScope();

                await scope.ServiceProvider.GetService<AuditService>().LogMessageEditedAsync(messageBefore, messageAfter, channel, guildChannel.Guild);
            }
        }

        public void Dispose()
        {
            Client.MessageUpdated -= OnMessageUpdatedAsync;
        }

        public void Init()
        {
            Client.MessageUpdated += OnMessageUpdatedAsync;
        }

        public async Task InitAsync() { }
    }
}
