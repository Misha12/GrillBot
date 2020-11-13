using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Grillbot.Extensions.Discord;
using Grillbot.Services.Config;
using Grillbot.Services.Logger.LoggerMethods.LogEmbed;
using Grillbot.Services.MessageCache;

namespace Grillbot.Services.Logger.LoggerMethods
{
    public class MessageEdited : LoggerMethodBase
    {
        public MessageEdited(DiscordSocketClient client, ConfigurationService config, IMessageCache messageCache)
            : base(client, messageCache, null, null, config)
        {
        }

        public async Task<bool> ProcessAsync(Cacheable<IMessage, ulong> messageBefore, SocketMessage messageAfter, ISocketMessageChannel channel)
        {
            var oldMessage = messageBefore.HasValue ? messageBefore.Value : MessageCache.Get(messageBefore.Id);
            if (!IsDetectedChange(oldMessage, messageAfter)) return false;

            var logEmbedBuilder = new LogEmbedBuilder("Zpráva byla upravena", LogEmbedType.MessageEdited);

            logEmbedBuilder
                .SetAuthor(messageAfter.Author)
                .AddCodeBlockField("Před", oldMessage.Content)
                .AddCodeBlockField("Po", messageAfter.Content)
                .AddField("Kanál", $"<#{channel.Id}> ({channel.Id})")
                .AddField("Odkaz", messageAfter.GetJumpUrl())
                .SetFooter($"MessageID: {messageAfter.Id} | AuthorID: {messageAfter.Author.Id}");

            await SendEmbedAsync(logEmbedBuilder).ConfigureAwait(false);

            MessageCache.Update(messageAfter);
            return true;
        }

        private bool IsDetectedChange(IMessage messageBefore, IMessage messageAfter)
        {
            return messageBefore != null && messageAfter != null
                && messageBefore.Author.IsUser() && messageBefore.Content != messageAfter.Content;
        }
    }
}
