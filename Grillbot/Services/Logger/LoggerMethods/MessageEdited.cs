using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Grillbot.Services.Config.Models;
using Grillbot.Services.Logger.LoggerMethods.LogEmbed;
using Grillbot.Services.MessageCache;

namespace Grillbot.Services.Logger.LoggerMethods
{
    public class MessageEdited : LoggerMethodBase
    {
        public MessageEdited(DiscordSocketClient client, Configuration config, IMessageCache messageCache) : base(client, config, messageCache)
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
                .SetTimestamp(true)
                .SetFooter($"MessageID: {messageAfter.Id} | AuthorID: {messageAfter.Author.Id}");

            var loggerRoom = GetLoggerRoom();
            await loggerRoom.SendMessageAsync(embed: logEmbedBuilder.Build());

            MessageCache.Update(messageAfter);
            return true;
        }

        private bool IsDetectedChange(IMessage messageBefore, IMessage messageAfter)
        {
            if (messageBefore == null) return false;
            if (messageAfter == null) return false;

            return messageBefore.Content != messageAfter.Content;
        }
    }
}
