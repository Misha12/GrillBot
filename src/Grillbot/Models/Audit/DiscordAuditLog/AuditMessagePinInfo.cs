using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Grillbot.Services.MessageCache;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Grillbot.Models.Audit.DiscordAuditLog
{
    public class AuditMessagePinInfo : IAuditLogData
    {
        [JsonProperty("m_id")]
        public ulong MessageId { get; set; }

        [JsonProperty("ch_id")]
        public ulong ChannelId { get; set; }

        [JsonIgnore]
        public IMessage Message { get; set; }

        [JsonIgnore]
        public IChannel Channel { get; set; }

        public AuditMessagePinInfo() { }

        public AuditMessagePinInfo(ulong messageId, ulong channelId)
        {
            MessageId = messageId;
            ChannelId = channelId;
        }

        public AuditMessagePinInfo(MessagePinAuditLogData data) : this(data.MessageId, data.ChannelId) { }
        public AuditMessagePinInfo(MessageUnpinAuditLogData data) : this(data.MessageId, data.ChannelId) { }

        public static IAuditLogData Create(IAuditLogData entryData)
        {
            if (entryData is MessageUnpinAuditLogData unpin)
                return new AuditMessagePinInfo(unpin);
            else if (entryData is MessagePinAuditLogData pin)
                return new AuditMessagePinInfo(pin);
            else
                return null;
        }

        public async Task<AuditMessagePinInfo> GetFilledModelAsync(SocketGuild guild, IMessageCache cache)
        {
            Message = await cache.GetAsync(ChannelId, MessageId);
            Channel = guild.GetChannel(ChannelId);
            return this;
        }
    }
}
