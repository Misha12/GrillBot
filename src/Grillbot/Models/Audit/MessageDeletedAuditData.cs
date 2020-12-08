using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;

namespace Grillbot.Models.Audit
{
    public class MessageDeletedAuditData
    {
        public bool IsInCache { get; set; }
        public ulong ChannelId { get; set; }
        public AuditUserInfo Author { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Content { get; set; }

        [JsonIgnore]
        public IChannel Channel { get; set; }

        public static MessageDeletedAuditData CreateDbItem(IChannel channel, IMessage message = null)
        {
            var data = new MessageDeletedAuditData()
            {
                ChannelId = channel.Id,
                IsInCache = message != null,
            };

            if (message == null)
                return data;

            data.Author = AuditUserInfo.Create(message.Author);
            data.CreatedAt = message.CreatedAt.LocalDateTime;
            data.Content = message.Content;
            return data;
        }

        public MessageDeletedAuditData GetFilledModel(SocketGuild guild)
        {
            Channel = guild.GetChannel(ChannelId);
            return this;
        }
    }
}
