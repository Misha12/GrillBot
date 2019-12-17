using Discord;
using Discord.WebSocket;
using Grillbot.Extensions.Discord;
using Grillbot.Services.Logger;
using Grillbot.Services.Statistics;
using System.Threading.Tasks;

namespace Grillbot.Handlers
{
    public class MessageEditedHandler : IHandle
    {
        private DiscordSocketClient Client { get; }
        private Logger Logger { get; }
        private CalledEventStats CalledEventStats { get; }

        public MessageEditedHandler(DiscordSocketClient client, Logger logger, CalledEventStats calledEventStats)
        {
            Client = client;
            Logger = logger;
            CalledEventStats = calledEventStats;

            Client.MessageUpdated += OnMessageUpdatedAsync;
        }

        private async Task OnMessageUpdatedAsync(Cacheable<IMessage, ulong> messageBefore, SocketMessage messageAfter, ISocketMessageChannel channel)
        {
            CalledEventStats.Increment("MessageUpdated");

            if (!messageAfter.Author.IsUser()) return;

            await Logger.OnMessageEdited(messageBefore, messageAfter, channel).ConfigureAwait(false);
        }

        public void Dispose()
        {
            Client.MessageUpdated -= OnMessageUpdatedAsync;
        }
    }
}
