using Discord.WebSocket;
using Grillbot.Services.Audit;
using Grillbot.Services.Initiable;
using Grillbot.Services.Statistics;
using Grillbot.Services.Unverify;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Grillbot.Handlers
{
    public class UserLeftHandler : IInitiable, IDisposable, IHandler
    {
        private DiscordSocketClient Client { get; }
        private InternalStatistics InternalStatistics { get; }
        private IServiceProvider Provider { get; }

        public UserLeftHandler(DiscordSocketClient client, InternalStatistics internalStatistics, IServiceProvider provider)
        {
            Client = client;
            InternalStatistics = internalStatistics;
            Provider = provider;
        }

        private async Task OnUserLeftAsync(SocketGuildUser user)
        {
            InternalStatistics.IncrementEvent("UserLeft");

            using var scope = Provider.CreateScope();
            await scope.ServiceProvider.GetService<UnverifyService>().OnUserLeftGuildAsync(user);
            await scope.ServiceProvider.GetService<AuditService>().LogUserLeftAsync(user);
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
