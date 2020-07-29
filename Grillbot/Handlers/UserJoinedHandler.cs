using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Grillbot.Extensions.Discord;
using Grillbot.Models.Config.AppSettings;
using Grillbot.Services;
using Grillbot.Services.Initiable;
using Grillbot.Services.Logger;
using Grillbot.Services.Statistics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Grillbot.Handlers
{
    public class UserJoinedHandler : IDisposable, IInitiable
    {
        private DiscordSocketClient Client { get; }
        private Configuration Config { get; }
        private Logger Logger { get; }
        private InternalStatistics InternalStatistics { get; }
        private IServiceProvider Services { get; }

        public UserJoinedHandler(DiscordSocketClient client, IOptions<Configuration> config, Logger logger, InternalStatistics internalStatistics,
            IServiceProvider services)
        {
            Client = client;
            Logger = logger;
            InternalStatistics = internalStatistics;
            Config = config.Value;
            Services = services;
        }

        private async Task OnUserJoinedOnServerAsync(SocketGuildUser user)
        {
            InternalStatistics.IncrementEvent("UserJoined");
            var message = Config.Discord.UserJoinedMessage;

            if (!string.IsNullOrEmpty(message))
                await user.SendPrivateMessageAsync(message).ConfigureAwait(false);

            await Logger.OnUserJoined(user).ConfigureAwait(false);

            using var scope = Services.CreateScope();
            using var inviteTracker = scope.ServiceProvider.GetService<InviteTrackerService>();

            await inviteTracker.OnUserJoinedAsync(user);
        }

        #region IDisposable Support

        public void Dispose()
        {
            Client.UserJoined -= OnUserJoinedOnServerAsync;
        }

        #endregion

        public void Init()
        {
            Client.UserJoined += OnUserJoinedOnServerAsync;
        }

        public async Task InitAsync() { }
    }
}
