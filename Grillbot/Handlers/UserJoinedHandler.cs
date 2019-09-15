using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Grillbot.Services.Config;
using Grillbot.Services.Config.Models;
using Grillbot.Services.Logger;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Grillbot.Handlers
{
    public class UserJoinedHandler : IConfigChangeable, IHandle
    {
        private DiscordSocketClient Client { get; }
        private Configuration Config { get; set; }
        private Logger Logger { get; }

        public UserJoinedHandler(DiscordSocketClient client, IOptions<Configuration> config, Logger logger)
        {
            Client = client;
            Logger = logger;
            
            //ConfigChanged(config); // TODO

            Client.UserJoined += OnUserJoinedOnServerAsync;
        }

        private async Task OnUserJoinedOnServerAsync(SocketGuildUser user)
        {
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

        public void ConfigChanged(IConfiguration newConfig)
        {
            //TODO
            //Config = newConfig;
        }
    }
}
