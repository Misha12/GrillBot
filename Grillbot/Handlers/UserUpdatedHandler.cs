using Discord.WebSocket;
using Grillbot.Services.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Handlers
{
    public class UserUpdatedHandler : IHandle
    {
        private DiscordSocketClient Client { get; }
        private Logger Logger { get; }

        public UserUpdatedHandler(DiscordSocketClient client, Logger logger)
        {
            Client = client;
            Logger = logger;

            Client.UserUpdated += OnUserUpdatedAsync;
        }

        private async Task OnUserUpdatedAsync(SocketUser userBefore, SocketUser userAfter)
        {
            await Logger.OnUserUpdated(userBefore, userAfter);
        }

        public void Dispose()
        {
            Client.UserUpdated -= OnUserUpdatedAsync;
        }
    }
}
