using Discord;
using Discord.Rest;
using Newtonsoft.Json;

namespace Grillbot.Models.Audit.DiscordAuditLog
{
    public class AuditUnban : IAuditLogData
    {
        [JsonProperty("user")]
        public AuditUserInfo User { get; set; }

        public static MappedAuditLogItem Create(IAuditLogData entryData)
        {
            if (entryData is not UnbanAuditLogData data)
                return null;

            var unbanData = new AuditUnban()
            {
                User = AuditUserInfo.Create(data.Target)
            };

            return new MappedAuditLogItem(null, unbanData);
        }
    }
}
