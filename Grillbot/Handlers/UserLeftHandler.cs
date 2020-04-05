using Discord.WebSocket;
using Grillbot.Services.Initiable;
using Grillbot.Services.Logger;
using Grillbot.Services.Statistics;
using System;
using System.Threading.Tasks;

namespace Grillbot.Handlers
{
    public class UserLeftHandler : IInitiable, IDisposable
    {
        private DiscordSocketClient Client { get; }
        private Logger Logger { get; }
        private InternalStatistics InternalStatistics { get; }

        public UserLeftHandler(DiscordSocketClient client, Logger logger, InternalStatistics internalStatistics)
        {
            Client = client;
            Logger = logger;
            InternalStatistics = internalStatistics;
        }

        private async Task OnUserLeftAsync(SocketGuildUser user)
        {
            InternalStatistics.IncrementEvent("UserLeft");
            await Logger.OnUserLeft(user).ConfigureAwait(false);
        }

        public void Dispose()
        {
            Client.UserLeft -= OnUserLeftAsync;
        }

        public void Init()
        {
            Client.UserLeft += OnUserLeftAsync;
        }

        public async Task InitAsync() { }
    }
}
