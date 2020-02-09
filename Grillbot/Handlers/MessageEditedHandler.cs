using Discord;
using Discord.WebSocket;
using Grillbot.Extensions.Discord;
using Grillbot.Services.Initiable;
using Grillbot.Services.Logger;
using Grillbot.Services.Statistics;
using System;
using System.Threading.Tasks;

namespace Grillbot.Handlers
{
    public class MessageEditedHandler : IDisposable, IInitiable
    {
        private DiscordSocketClient Client { get; }
        private Logger Logger { get; }
        private CalledEventStats CalledEventStats { get; }

        public MessageEditedHandler(DiscordSocketClient client, Logger logger, CalledEventStats calledEventStats)
        {
            Client = client;
            Logger = logger;
            CalledEventStats = calledEventStats;
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

        public void Init()
        {
            Client.MessageUpdated += OnMessageUpdatedAsync;
        }

        public async Task InitAsync() { }
    }
}
