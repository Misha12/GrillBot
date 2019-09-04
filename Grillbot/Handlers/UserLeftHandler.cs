using Discord.WebSocket;
using Grillbot.Services.Logger;
using System.Threading.Tasks;

namespace Grillbot.Handlers
{
    public class UserLeftHandler : IHandle
    {
        private DiscordSocketClient Client { get; }
        private Logger Logger { get; }

        public UserLeftHandler(DiscordSocketClient client, Logger logger)
        {
            Client = client;
            Logger = logger;

            Client.UserLeft += OnUserLeftAsync;
        }

        private async Task OnUserLeftAsync(SocketGuildUser user)
        {
            await Logger.OnUserLeft(user);
        }

        public void Dispose()
        {
            Client.UserLeft -= OnUserLeftAsync;
        }
    }
}
