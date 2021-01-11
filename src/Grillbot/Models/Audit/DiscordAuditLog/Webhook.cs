using Discord;
using Discord.Rest;
using Newtonsoft.Json;

namespace Grillbot.Models.Audit.DiscordAuditLog
{
    public class Webhook : IAuditLogData
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        public Webhook() { }

        public Webhook(string name)
        {
            Name = name;
        }

        public Webhook(WebhookCreateAuditLogData data) : this(data.Name) { }
        public Webhook(WebhookDeleteAuditLogData data) : this(data.Name) { }

        public static MappedAuditLogItem Create(IAuditLogData entryData)
        {
            if (entryData is WebhookCreateAuditLogData createData)
                return new MappedAuditLogItem(createData.ChannelId, new Webhook(createData));
            else if (entryData is WebhookDeleteAuditLogData deleteData)
                return new MappedAuditLogItem(deleteData.ChannelId, new Webhook(deleteData));
            else
                return null;
        }
    }
}
