using System.Threading.Tasks;
using Discord.WebSocket;
using Grillbot.Extensions.Discord;
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
            var message = Config.Discord.UserJoinedMessage;

            if (!string.IsNullOrEmpty(message))
                await user.SendPrivateMessageAsync(message).ConfigureAwait(false);

            await Logger.OnUserJoined(user).ConfigureAwait(false);
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
