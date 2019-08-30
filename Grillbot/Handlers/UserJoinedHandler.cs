using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Grillbot.Services.Config;
using Grillbot.Services.Logger;
using Microsoft.Extensions.Configuration;

namespace Grillbot.Handlers
{
    public class UserJoinedHandler : IConfigChangeable, IDisposable
    {
        private DiscordSocketClient Client { get; }
        private IConfiguration Config { get; set; }
        private Logger Logger { get; }

        public UserJoinedHandler(DiscordSocketClient client, IConfiguration config, Logger logger)
        {
            Client = client;
            Logger = logger;
            
            ConfigChanged(config);

            Client.UserJoined += OnUserJoinedOnServerAsync;
        }

        private async Task OnUserJoinedOnServerAsync(SocketGuildUser user)
        {
            if (user.IsBot || user.IsWebhook) return;
            var message = Config["Discord:UserJoinedMessage"];

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

        public void ConfigChanged(IConfiguration newConfig)
        {
            Config = newConfig;
        }
    }
}
