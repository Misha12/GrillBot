using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Grillbot.Services.Logger.LoggerMethods.LogEmbed;
using Grillbot.Services.MessageCache;
using Microsoft.Extensions.Configuration;

namespace Grillbot.Services.Logger.LoggerMethods
{
    public class MessageEdited : LoggerMethodBase
    {
        public MessageEdited(DiscordSocketClient client, IConfiguration config, IMessageCache messageCache) : base(client, config, messageCache)
        {
        }

        public async Task ProcessAsync(Cacheable<IMessage, ulong> messageBefore, SocketMessage messageAfter, ISocketMessageChannel channel)
        {
            var oldMessage = messageBefore.HasValue ? messageBefore.Value : MessageCache.Get(messageBefore.Id);
            var logEmbedBuilder = new LogEmbedBuilder("Zpráva byla upravena", LogEmbedType.MessageEdited);

            logEmbedBuilder
                .SetAuthor(messageAfter.Author)
                .AddField("Před", $"```{oldMessage.Content}```")
                .AddField("Po", $"```{messageAfter.Content}```")
                .AddField("Kanál", $"<#{channel.Id}> ({channel.Id})")
                .AddField("Odkaz", messageAfter.GetJumpUrl())
                .SetTimestamp(true)
                .SetFooter($"MessageID: {messageAfter.Id} | AuthorID: {messageAfter.Author.Id}");

            var loggerRoom = GetLoggerRoom();
            await loggerRoom.SendMessageAsync(embed: logEmbedBuilder.Build());

            MessageCache.Update(messageAfter);
        }
    }
}
