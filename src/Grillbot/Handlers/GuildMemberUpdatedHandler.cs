using Discord.WebSocket;
using Grillbot.Services.Initiable;
using Grillbot.Services.Logger;
using Grillbot.Services.Statistics;
using System;
using System.Threading.Tasks;

namespace Grillbot.Handlers
{
    public class GuildMemberUpdatedHandler : IInitiable, IDisposable
    {
        private DiscordSocketClient Client { get; }
        private Logger Logger { get; }
        private InternalStatistics InternalStatistics { get; }

        public GuildMemberUpdatedHandler(DiscordSocketClient client, Logger logger, InternalStatistics internalStatistics)
        {
            Logger = logger;
            Client = client;
            InternalStatistics = internalStatistics;

            Client.GuildMemberUpdated += OnGuildMemberUpdatedAsync;
        }

        private async Task OnGuildMemberUpdatedAsync(SocketGuildUser guildUserBefore, SocketGuildUser guildUserAfter)
        {
            InternalStatistics.IncrementEvent("GuildMemberUpdated");
            await Logger.OnGuildMemberUpdatedAsync(guildUserBefore, guildUserAfter).ConfigureAwait(false);
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
