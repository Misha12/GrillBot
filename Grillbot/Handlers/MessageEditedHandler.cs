using Discord;
using Discord.WebSocket;
using Grillbot.Services.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Handlers
{
    public class MessageEditedHandler : IDisposable
    {
        private DiscordSocketClient Client { get; }
        private Logger Logger { get; }

        public MessageEditedHandler(DiscordSocketClient client, Logger logger)
        {
            Client = client;
            Logger = logger;

            Client.MessageUpdated += OnMessageUpdatedAsync;
        }

        private async Task OnMessageUpdatedAsync(Cacheable<IMessage, ulong> messageBefore, SocketMessage messageAfter, ISocketMessageChannel channel)
        {
            if (messageAfter.Author.IsBot || messageAfter.Author.IsWebhook) return;

            await Logger.OnMessageEdited(messageBefore, messageAfter, channel);
        }

        public void Dispose()
        {
            Client.MessageUpdated -= OnMessageUpdatedAsync;
        }
    }
}
