using Discord;
using Newtonsoft.Json;
using System;

namespace Grillbot.Models.Audit
{
    public class ReplyContent
    {
        [JsonProperty("url")]
        public string JumpUrl { get; set; }

        [JsonProperty("author")]
        public AuditUserInfo Author { get; set; }

        [JsonProperty("cnt")]
        public string Content { get; set; }

        [JsonProperty("created")]
        public DateTime CreatedAt { get; set; }

        public ReplyContent() { }

        public ReplyContent(string jumpUrl, AuditUserInfo author, string content, DateTime createdAt)
        {
            JumpUrl = jumpUrl;
            Author = author;
            Content = content;
            CreatedAt = createdAt;
        }

        public static ReplyContent Create(IMessage message)
        {
            if (message == null)
                return null;

            return new ReplyContent(message.GetJumpUrl(), AuditUserInfo.Create(message.Author), message.Content, message.CreatedAt.LocalDateTime);
        }
    }
}
