using Discord.WebSocket;
using Grillbot.Services.Logger;
using Grillbot.Services.Statistics;
using System.Threading.Tasks;

namespace Grillbot.Handlers
{
    public class GuildMemberUpdatedHandler : IHandle
    {
        private DiscordSocketClient Client { get; }
        private Logger Logger { get; }
        private CalledEventStats CalledEventStats { get; }

        public GuildMemberUpdatedHandler(DiscordSocketClient client, Logger logger, CalledEventStats calledEventStats)
        {
            Logger = logger;
            Client = client;
            CalledEventStats = calledEventStats;

            Client.GuildMemberUpdated += OnGuildMemberUpdatedAsync;
        }

        private async Task OnGuildMemberUpdatedAsync(SocketGuildUser guildUserBefore, SocketGuildUser guildUserAfter)
        {
            CalledEventStats.Increment("GuildMemberUpdated");

            await Logger.OnGuildMemberUpdatedAsync(guildUserBefore, guildUserAfter);
        }

        public void Dispose()
        {
            Client.GuildMemberUpdated -= OnGuildMemberUpdatedAsync;
        }
    }
}
