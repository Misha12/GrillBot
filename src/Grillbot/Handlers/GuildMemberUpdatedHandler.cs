using Discord.WebSocket;
using Grillbot.Services.Audit;
using Grillbot.Services.Initiable;
using Grillbot.Services.Statistics;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Grillbot.Handlers
{
    public class GuildMemberUpdatedHandler : IInitiable, IDisposable
    {
        private DiscordSocketClient Client { get; }
        private InternalStatistics InternalStatistics { get; }
        private IServiceProvider Provider { get; }

        private DateTime LastEventAt { get; set; }

        public GuildMemberUpdatedHandler(DiscordSocketClient client, InternalStatistics internalStatistics, IServiceProvider provider)
        {
            Client = client;
            InternalStatistics = internalStatistics;
            Provider = provider;

            Client.GuildMemberUpdated += OnGuildMemberUpdatedAsync;
        }

        private async Task OnGuildMemberUpdatedAsync(SocketGuildUser guildUserBefore, SocketGuildUser guildUserAfter)
        {
            if (LastEventAt != DateTime.MinValue && (DateTime.UtcNow - LastEventAt).TotalSeconds < 1.0D)
                return;

            InternalStatistics.IncrementEvent("GuildMemberUpdated");

            using var scope = Provider.CreateScope();
            await scope.ServiceProvider.GetService<AuditService>().ProcessBoostChangeAsync(guildUserBefore, guildUserAfter);

            LastEventAt = DateTime.UtcNow;
        }

        public void Dispose()
        {
            Client.GuildMemberUpdated -= OnGuildMemberUpdatedAsync;
        }

        public void Init()
        {
            Client.GuildMemberUpdated += OnGuildMemberUpdatedAsync;
        }

        public async Task InitAsync() { }
    }
}
