using Discord;
using Discord.Rest;
using Newtonsoft.Json;

namespace Grillbot.Models.Audit.DiscordAuditLog
{
    public class AuditPruneMembers : IAuditLogData
    {
        [JsonProperty("days")]
        public int PruneDays { get; set; }

        [JsonProperty("count")]
        public int MembersRemoved { get; set; }

        public static MappedAuditLogItem Create(IAuditLogData entryData)
        {
            if (entryData is not PruneAuditLogData data)
                return null;

            var pruneData = new AuditPruneMembers()
            {
                MembersRemoved = data.MembersRemoved,
                PruneDays = data.PruneDays
            };

            return new MappedAuditLogItem(null, pruneData);
        }
    }
}
