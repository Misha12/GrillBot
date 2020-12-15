using Discord;
using Discord.WebSocket;
using Grillbot.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Text.RegularExpressions;

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

        public static MessageDeletedAuditData Create(ImmutableArray<EmbedField> fields)
        {
            if (fields.Length < 3)
                return null;

            var userIdentification = fields[0].Value.Split("#");

            if (userIdentification.Length < 2)
                return null;

            var author = new AuditUserInfo()
            {
                Discriminator = userIdentification[1],
                Username = userIdentification[0]
            };

            int channelIdIndex = fields.Length == 3 ? 1 : 2;
            int contentIndex = fields.Length == 3 ? 2 : 4;

            var baseData = Create(fields[channelIdIndex]);
            baseData.IsInCache = true;
            baseData.Author = author;
            baseData.CreatedAt = fields.Length > 3 && DateTime.TryParseExact(fields[1].Value, "dd. MM. yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateTime) ? dateTime : DateTime.MinValue;
            baseData.Content = fields[contentIndex].Value.ClearCodeBlocks();

            return baseData;
        }

        public static MessageDeletedAuditData Create(EmbedField field)
        {
            var channelIdRegex = new Regex(@".*\s\((\d*)\)", RegexOptions.IgnoreCase);
            var channelIdMatch = channelIdRegex.Match(field.Value);

            return new MessageDeletedAuditData(channelIdMatch.Success ? Convert.ToUInt64(channelIdMatch.Groups[1].Value) : 0, false);
        }

        public MessageDeletedAuditData GetFilledModel(SocketGuild guild)
        {
            Channel = guild.GetChannel(ChannelId);
            return this;
        }
    }
}
