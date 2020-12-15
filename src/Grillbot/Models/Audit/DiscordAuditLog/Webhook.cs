using Discord;
using Discord.Rest;
using Grillbot.Enums;
using Newtonsoft.Json;

namespace Grillbot.Models.Audit.DiscordAuditLog
{
    public class Webhook : IAuditLogData
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("ch_id")]
        public ulong ChannelId { get; set; }

        [JsonIgnore]
        public IChannel Channel { get; set; }

        public Webhook() { }

        public Webhook(string name, ulong channelId)
        {
            Name = name;
            ChannelId = channelId;
        }

        public Webhook(WebhookCreateAuditLogData data) : this(data.Name, data.ChannelId) { }
        public Webhook(WebhookDeleteAuditLogData data) : this(data.Name, data.ChannelId) { }

        public static IAuditLogData Create(IAuditLogData entryData)
        {
            if (entryData is WebhookCreateAuditLogData createData)
                return new Webhook(createData);
            else if (entryData is WebhookDeleteAuditLogData deleteData)
                return new Webhook(deleteData);
            else
                return null;
        }

        public static Webhook FromJsonIfValid(AuditLogType type, string json)
        {
            if (type != AuditLogType.WebhookCreated && type != AuditLogType.WebhookDeleted)
                return null;

            return JsonConvert.DeserializeObject<Webhook>(json);
        }
    }
}
