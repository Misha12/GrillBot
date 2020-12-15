using Discord;
using Discord.Rest;
using Grillbot.Enums;
using Newtonsoft.Json;

namespace Grillbot.Models.Audit
{
    public class AuditBotAdded : IAuditLogData
    {
        [JsonProperty("bot")]
        public AuditUserInfo Bot { get; set; }

        public static IAuditLogData Create(IAuditLogData entryData)
        {
            if (entryData is not BotAddAuditLogData data)
                return null;

            return new AuditBotAdded()
            {
                Bot = AuditUserInfo.Create(data.Target)
            };
        }

        public static AuditBotAdded FromJsonIfValid(AuditLogType type, string json)
        {
            return type == AuditLogType.BotAdded ? JsonConvert.DeserializeObject<AuditBotAdded>(json) : null;
        }
    }
}
