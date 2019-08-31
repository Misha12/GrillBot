using Discord;
using Discord.WebSocket;
using Grillbot.Services.Logger.LoggerMethods;
using Grillbot.Services.MessageCache;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace Grillbot.Services.Logger
{
    public class Logger : IDisposable
    {
        private DiscordSocketClient Client { get; }
        private IConfiguration Config { get; }
        private IMessageCache MessageCache { get; }

        public Logger(DiscordSocketClient client, IConfiguration config, IMessageCache messageCache)
        {
            Client = client;
            Config = config;
            MessageCache = messageCache;
        }

        public async Task OnGuildMemberUpdatedAsync(SocketGuildUser userBefore, SocketGuildUser userAfter)
        {
            // TODO
        }

        public async Task OnMessageDelete(Cacheable<IMessage, ulong> message, ISocketMessageChannel channel)
        {
            var method = new MessageDeleted(Client, Config, MessageCache);
            await method.ProcessAsync(message, channel);
        }

        public async Task OnMessageUpdated(Cacheable<IMessage, UInt64> messageBefore, SocketMessage messageAfter, ISocketMessageChannel channel)
        {
            //TODO
        }

        public async Task OnReactionsCleared(Cacheable<IUserMessage, UInt64> message, ISocketMessageChannel channel)
        {
            //TODO
        }

        public async Task OnRoleCreated(SocketRole role)
        {
            //TODO
        }

        public async Task OnRoleDeleted(SocketRole role)
        {
            //TODO
        }

        public async Task OnRoleUpdated(SocketRole before, SocketRole after)
        {
            //TODO
        }

        public async Task OnUserBanned(SocketUser user, SocketGuild guild)
        {
            //TODO
        }

        public async Task OnUserJoined(SocketGuildUser user)
        {
            //TODO
        }

        public async Task OnUserLeft(SocketGuildUser user)
        {
            //TODO
        }

        public async Task OnUserUnbanned(SocketUser user, SocketGuild guild)
        {
            //TODO
        }

        public async Task OnUserUpdated(SocketUser userBefore, SocketUser userAfter)
        {
            //TODO
        }

        public async Task OnUserVoiceStateUpdated(SocketUser user, SocketVoiceState oldState, SocketVoiceState newState)
        {
            //TODO
        }

        public async Task OnVoiceServerUpdated(SocketVoiceServer server)
        {
            //TODO
        }

        public void Dispose()
        {
        }
    }
}
