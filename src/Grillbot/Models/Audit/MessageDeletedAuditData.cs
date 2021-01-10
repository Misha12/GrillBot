using Discord;
using Newtonsoft.Json;
using System;

namespace Grillbot.Models.Audit
{
    public class MessageDeletedAuditData
    {
        [JsonProperty("cache")]
        public bool IsInCache { get; set; }

        [JsonProperty("author")]
        public AuditUserInfo Author { get; set; }

        [JsonProperty("created")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }

        public MessageDeletedAuditData() { }

        public MessageDeletedAuditData(bool isInCache)
        {
            IsInCache = isInCache;
        }

        public MessageDeletedAuditData(bool isInCache, AuditUserInfo author, DateTime createdAt, string content) : this(isInCache)
        {
            Author = author;
            CreatedAt = createdAt;
            Content = content;
        }

        public static MessageDeletedAuditData Create(IMessage message = null)
        {
            if (message == null)
                return new MessageDeletedAuditData(false);
            else
                return new MessageDeletedAuditData(true, AuditUserInfo.Create(message.Author), message.CreatedAt.LocalDateTime, message.Content);
        }
    }
}
