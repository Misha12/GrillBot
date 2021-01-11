using Discord;
using Discord.Rest;
using Newtonsoft.Json;

namespace Grillbot.Models.Audit
{
    public class AuditBotAdded : IAuditLogData
    {
        [JsonProperty("bot")]
        public AuditUserInfo Bot { get; set; }

        public static MappedAuditLogItem Create(IAuditLogData entryData)
        {
            if (entryData is not BotAddAuditLogData data)
                return null;

            var botAddData = new AuditBotAdded()
            {
                Bot = AuditUserInfo.Create(data.Target)
            };

            return new MappedAuditLogItem(null, botAddData);
        }
    }
}
