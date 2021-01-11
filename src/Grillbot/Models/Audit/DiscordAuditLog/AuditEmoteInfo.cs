using Discord;
using Discord.Rest;
using Newtonsoft.Json;

namespace Grillbot.Models.Audit.DiscordAuditLog
{
    public class AuditEmoteInfo : IAuditLogData
    {
        [JsonProperty("id")]
        public ulong Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        public AuditEmoteInfo() { }

        public AuditEmoteInfo(ulong id, string name)
        {
            Id = id;
            Name = name;
        }

        public AuditEmoteInfo(EmoteCreateAuditLogData data) : this(data.EmoteId, data.Name) { }
        public AuditEmoteInfo(EmoteDeleteAuditLogData data) : this(data.EmoteId, data.Name) { }

        public static MappedAuditLogItem Create(IAuditLogData entryData)
        {
            if (entryData is EmoteCreateAuditLogData data)
                return new MappedAuditLogItem(null, new AuditEmoteInfo(data));
            else if (entryData is EmoteDeleteAuditLogData deleteData)
                return new MappedAuditLogItem(null, new AuditEmoteInfo(deleteData));
            else
                return null;
        }
    }
}
