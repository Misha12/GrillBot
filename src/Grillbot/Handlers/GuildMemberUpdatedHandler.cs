using Discord;
using Discord.WebSocket;
using Grillbot.Extensions.Infrastructure;
using Grillbot.Models.Audit.DiscordAuditLog;
using Grillbot.Services.Audit;
using Grillbot.Services.BackgroundTasks;
using Grillbot.Services.Initiable;
using Grillbot.Services.Statistics;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Grillbot.Handlers
{
    public class GuildMemberUpdatedHandler : IInitiable, IDisposable, IHandler
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

            if (AuditMemberUpdated.IsSchedulable(guildUserBefore, guildUserAfter))
                scope.ServiceProvider.GetService<BackgroundTaskQueue>().ScheduleDownloadAuditLogIfNotExists(ActionType.MemberUpdated, guildUserAfter.Guild, 120);

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
