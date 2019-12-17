using Discord.WebSocket;
using Grillbot.Services.Logger;
using Grillbot.Services.Statistics;
using System.Threading.Tasks;

namespace Grillbot.Handlers
{
    public class UserLeftHandler : IHandle
    {
        private DiscordSocketClient Client { get; }
        private Logger Logger { get; }
        private CalledEventStats CalledEventStats { get; }

        public UserLeftHandler(DiscordSocketClient client, Logger logger, CalledEventStats calledEventStats)
        {
            Client = client;
            Logger = logger;
            CalledEventStats = calledEventStats;

            Client.UserLeft += OnUserLeftAsync;
        }

        private async Task OnUserLeftAsync(SocketGuildUser user)
        {
            CalledEventStats.Increment("UserLeft");
            await Logger.OnUserLeft(user).ConfigureAwait(false);
        }

        public void Dispose()
        {
            Client.UserLeft -= OnUserLeftAsync;
        }
    }
}
