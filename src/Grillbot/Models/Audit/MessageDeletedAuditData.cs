using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;

namespace Grillbot.Models.Audit
{
    public class MessageDeletedAuditData
    {
        [JsonProperty("cache")]
        public bool IsInCache { get; set; }

        [JsonProperty("ch_id")]
        public ulong ChannelId { get; set; }

        [JsonProperty("author")]
        public AuditUserInfo Author { get; set; }

        [JsonProperty("created")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonIgnore]
        public IChannel Channel { get; set; }

        public MessageDeletedAuditData() { }

        public MessageDeletedAuditData(ulong channelId, bool isInCache)
        {
            ChannelId = channelId;
            IsInCache = isInCache;
        }

        public MessageDeletedAuditData(ulong channelId, bool isInCache, AuditUserInfo author, DateTime createdAt, string content) : this(channelId, isInCache)
        {
            Author = author;
            CreatedAt = createdAt;
            Content = content;
        }

        public static MessageDeletedAuditData Create(IChannel channel, IMessage message = null)
        {
            if (message == null)
                return new MessageDeletedAuditData(channel.Id, false);
            else
                return new MessageDeletedAuditData(channel.Id, true, AuditUserInfo.Create(message.Author), message.CreatedAt.LocalDateTime, message.Content);
        }

        public MessageDeletedAuditData GetFilledModel(SocketGuild guild)
        {
            Channel = guild.GetChannel(ChannelId);
            return this;
        }
    }
}
