using System.Threading.Tasks;
using Discord.WebSocket;
using Grillbot.Repository.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Grillbot.Repository
{
    public class LoggerCacheRepository : RepositoryBase
    {
        public LoggerCacheRepository(IConfiguration config) : base(config)
        {
        }

        public async Task InserMessageToCache(SocketUserMessage message)
        {
            var loggerMessage = new LoggerMessage
            {
                SnowflakeAuthorID = message.Author.Id,
                SnowflakeChannelID = message.Channel.Id,
                SnowflakeMessageID = message.Id,
                Content = message.Content,
                CreatedAt = message.CreatedAt.UtcDateTime
            };

            foreach(var attachment in message.Attachments)
            {
                loggerMessage.Attachments.Add(new Entity.LoggerAttachment()
                {
                    SnowflakeAttachmentID = attachment.Id,
                    ProxyUrl = attachment.ProxyUrl,
                    UrlLink = attachment.Url,
                });
            }

            await Context.AddAsync(loggerMessage);
            await Context.SaveChangesAsync();
        }

        public async Task<LoggerMessage> GetMessage(ulong messageID)
        {
            return await Context.LoggerMessages
                .Include(o => o.Attachments)
                .FirstOrDefaultAsync(o => o.MessageID == messageID.ToString());
        }

        public async Task DeleteMessageFromCache(LoggerMessage message)
        {
            foreach(var attachment in message.Attachments)
            {
                Context.Remove(attachment);
            }

            Context.Remove(message);
            await Context.SaveChangesAsync();
        }
    }
}
