using Discord;
using Discord.Rest;
using Newtonsoft.Json;

namespace Grillbot.Models.Audit.DiscordAuditLog
{
    public class AuditEmoteUpdated : IAuditLogData
    {
        [JsonProperty("id")]
        public ulong Id { get; set; }

        [JsonProperty("name")]
        public DiffData<string> Name { get; set; }

        public static MappedAuditLogItem Create(IAuditLogData entryData)
        {
            if (entryData is not EmoteUpdateAuditLogData data)
                return null;

            var updateData = new AuditEmoteUpdated()
            {
                Id = data.EmoteId,
                Name = data.NewName != data.OldName ? new DiffData<string>(data.OldName, data.NewName) : null
            };

            return new MappedAuditLogItem(null, updateData);
        }
    }
}
