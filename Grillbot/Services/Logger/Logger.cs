using Discord;
using Discord.WebSocket;
using Grillbot.Services.Logger.LoggerMethods;
using Grillbot.Services.MessageCache;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace Grillbot.Services.Logger
{
    public class Logger
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

        public async Task OnMessageEdited(Cacheable<IMessage, ulong> messageBefore, SocketMessage messageAfter, ISocketMessageChannel channel)
        {
            var method = new MessageEdited(Client, Config, MessageCache);
            await method.Process(messageBefore, messageAfter, channel);
        }

        public async Task OnReactionsCleared(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel)
        {
            //TODO
        }

        public async Task OnUserJoined(SocketGuildUser user)
        {
            var method = new UserJoined(Client, Config);
            await method.Process(user);
        }

        public async Task OnUserLeft(SocketGuildUser user)
        {
            var method = new UserLeft(Client, Config);
            await method.Process(user);
        }

        public async Task OnUserUpdated(SocketUser userBefore, SocketUser userAfter)
        {
            //TODO
        }
    }
}
