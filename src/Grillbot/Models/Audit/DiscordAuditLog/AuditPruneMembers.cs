using Discord;
using Discord.Rest;
using Grillbot.Enums;
using Newtonsoft.Json;

namespace Grillbot.Models.Audit.DiscordAuditLog
{
    public class AuditPruneMembers : IAuditLogData
    {
        [JsonProperty("days")]
        public int PruneDays { get; set; }

        [JsonProperty("count")]
        public int MembersRemoved { get; set; }

        public static IAuditLogData Create(IAuditLogData entryData)
        {
            if (entryData is not PruneAuditLogData data)
                return null;

            return new AuditPruneMembers()
            {
                MembersRemoved = data.MembersRemoved,
                PruneDays = data.PruneDays
            };
        }

        public static AuditPruneMembers FromJsonIfValid(AuditLogType type, string json)
        {
            return type == AuditLogType.Prune ? JsonConvert.DeserializeObject<AuditPruneMembers>(json) : null;
        }
    }
}
