using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Grillbot.Services.Config;
using Grillbot.Services.Config.Models;
using Grillbot.Services.Logger;
using Grillbot.Services.Statistics;
using Microsoft.Extensions.Options;

namespace Grillbot.Handlers
{
    public class UserJoinedHandler : IConfigChangeable, IHandle
    {
        private DiscordSocketClient Client { get; }
        private Configuration Config { get; set; }
        private Logger Logger { get; }
        private CalledEventStats CalledEventStats { get; }

        public UserJoinedHandler(DiscordSocketClient client, IOptions<Configuration> config, Logger logger, CalledEventStats calledEventStats)
        {
            Client = client;
            Logger = logger;
            CalledEventStats = calledEventStats;
            
            ConfigChanged(config.Value);

            Client.UserJoined += OnUserJoinedOnServerAsync;
        }

        private async Task OnUserJoinedOnServerAsync(SocketGuildUser user)
        {
            CalledEventStats.Increment("UserJoined");

            if (user.IsBot || user.IsWebhook) return;
            var message = Config.Discord.UserJoinedMessage;

            if (!string.IsNullOrEmpty(message))
                await user.SendMessageAsync(message);

            await Logger.OnUserJoined(user);
        }

        #region IDisposable Support

        public void Dispose()
        {
            Client.UserJoined -= OnUserJoinedOnServerAsync;
        }

        #endregion

        public void ConfigChanged(Configuration newConfig)
        {
            Config = newConfig;
        }
    }
}
