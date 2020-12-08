using Discord;
using Discord.Rest;
using Newtonsoft.Json;

namespace Grillbot.Models.Audit.DiscordAuditLog
{
    public class AuditUnban : IAuditLogData
    {
        [JsonProperty("user")]
        public AuditUserInfo User { get; set; }

        public static IAuditLogData Create(IAuditLogData entryData)
        {
            if (entryData is not UnbanAuditLogData data)
                return null;

            return new AuditUnban()
            {
                User = AuditUserInfo.Create(data.Target)
            };
        }
    }
}
