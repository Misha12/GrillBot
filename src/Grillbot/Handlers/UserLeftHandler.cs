using Discord.WebSocket;
using Grillbot.Services.Initiable;
using Grillbot.Services.Logger;
using Grillbot.Services.Statistics;
using Grillbot.Services.Unverify;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Grillbot.Handlers
{
    public class UserLeftHandler : IInitiable, IDisposable
    {
        private DiscordSocketClient Client { get; }
        private Logger Logger { get; }
        private InternalStatistics InternalStatistics { get; }
        private IServiceProvider Provider { get; }

        public UserLeftHandler(DiscordSocketClient client, Logger logger, InternalStatistics internalStatistics, IServiceProvider provider)
        {
            Client = client;
            Logger = logger;
            InternalStatistics = internalStatistics;
            Provider = provider;
        }

        private async Task OnUserLeftAsync(SocketGuildUser user)
        {
            InternalStatistics.IncrementEvent("UserLeft");
            await Logger.OnUserLeft(user).ConfigureAwait(false);

            await RemoveUnverifyIfExistsAsync(user);
        }

        private async Task RemoveUnverifyIfExistsAsync(SocketGuildUser user)
        {
            using var scope = Provider.CreateScope();
            using var service = scope.ServiceProvider.GetService<UnverifyService>();

            await service.OnUserLeftGuildAsync(user);
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
