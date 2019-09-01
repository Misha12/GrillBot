using Discord.WebSocket;
using Grillbot.Services.Logger;
using System.Threading.Tasks;

namespace Grillbot.Handlers
{
    public class GuildMemberUpdatedHandler : IHandle
    {
        private DiscordSocketClient Client { get; }
        private Logger Logger { get; }

        public GuildMemberUpdatedHandler(DiscordSocketClient client, Logger logger)
        {
            Logger = logger;
            Client = client;

            Client.GuildMemberUpdated += OnGuildMemberUpdatedAsync;
        }

        private async Task OnGuildMemberUpdatedAsync(SocketGuildUser guildUserBefore, SocketGuildUser guildUserAfter)
        {
            await Logger.OnGuildMemberUpdatedAsync(guildUserBefore, guildUserAfter);
        }

        public void Dispose()
        {
            Client.GuildMemberUpdated -= OnGuildMemberUpdatedAsync;
        }
    }
}
