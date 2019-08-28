using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Grillbot.Services.Config;
using Microsoft.Extensions.Configuration;

namespace Grillbot.Handlers
{
    public class UserJoinedHandler : IConfigChangeable, IDisposable
    {
        private DiscordSocketClient Client { get; }
        private IConfiguration Config { get; set; }

        public UserJoinedHandler(DiscordSocketClient client, IConfiguration config)
        {
            Client = client;
            ConfigChanged(config);

            Client.UserJoined += OnUserJoinedOnServerAsync;
        }

        private async Task OnUserJoinedOnServerAsync(SocketGuildUser user)
        {
            if (user.IsBot || user.IsWebhook) return;
            var message = Config["Discord:UserJoinedMessage"];

            if (!string.IsNullOrEmpty(message))
                await user.SendMessageAsync(message);
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
