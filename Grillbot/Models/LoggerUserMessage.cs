using System;
using System.Collections.Generic;

namespace Grillbot.Models
{
    public class LoggerUserMessage
    {
        public ulong MessageID { get; set; }
        public ulong AuthorID { get; set; }
        public ulong ChannelID { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }

        public ICollection<LoggerAttachment> Attachments { get; }

        public override bool Equals(object obj)
        {
            if (!(obj is LoggerUserMessage lum))
                return false;

            return lum.MessageID == MessageID;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(MessageID);
        }

        public void SetValues(object authorID, object messageID, object content, object createdAt, object channelID)
        {
            if (MessageID == default)
                MessageID = Convert.ToUInt64(messageID);

            if (AuthorID == default)
                AuthorID = Convert.ToUInt64(authorID);

            if (Content == default)
                Content = content?.ToString();

            if (CreatedAt == default)
                CreatedAt = Convert.ToDateTime(createdAt);

            if (ChannelID == default)
                ChannelID = Convert.ToUInt64(channelID);
        }

        public void AddAttachment(object attachmentID, object urlLink, object proxyUrl)
        {
            var attachment = new LoggerAttachment()
            {
                AttachmentID = Convert.ToUInt64(attachmentID),
                MessageID = MessageID,
                ProxyUrl = proxyUrl?.ToString(),
                UrlLink = urlLink?.ToString()
            };

            Attachments.Add(attachment);
        }

        public LoggerUserMessage()
        {
            Attachments = new HashSet<LoggerAttachment>();
        }
    }
}
