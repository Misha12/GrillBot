using Discord;
using Discord.Rest;
using Grillbot.Services.MessageCache;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Grillbot.Models.Audit.DiscordAuditLog
{
    public class AuditMessagePinInfo : IAuditLogData
    {
        [JsonProperty("m_id")]
        public ulong MessageId { get; set; }

        [JsonIgnore]
        public IMessage Message { get; set; }

        public AuditMessagePinInfo() { }

        public AuditMessagePinInfo(ulong messageId)
        {
            MessageId = messageId;
        }

        public AuditMessagePinInfo(MessagePinAuditLogData data) : this(data.MessageId) { }
        public AuditMessagePinInfo(MessageUnpinAuditLogData data) : this(data.MessageId) { }

        public static MappedAuditLogItem Create(IAuditLogData entryData)
        {
            if (entryData is MessageUnpinAuditLogData unpin)
                return new MappedAuditLogItem(unpin.ChannelId, new AuditMessagePinInfo(unpin));
            else if (entryData is MessagePinAuditLogData pin)
                return new MappedAuditLogItem(pin.ChannelId, new AuditMessagePinInfo(pin));
            else
                return null;
        }

        public async Task<AuditMessagePinInfo> GetFilledModelAsync(IMessageCache cache, ulong channelId)
        {
            Message = await cache.GetAsync(channelId, MessageId);
            return this;
        }
    }
}
