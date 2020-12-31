using Discord;
using Discord.Rest;
using Newtonsoft.Json;

namespace Grillbot.Models.Audit.DiscordAuditLog
{
    public class AuditChannelUpdated : IAuditLogData
    {
        [JsonProperty("name")]
        public DiffData<string> Name { get; set; }

        [JsonProperty("topic")]
        public DiffData<string> Topic { get; set; }

        [JsonProperty("slowmode")]
        public DiffData<int?> SlowModeInterval { get; set; }

        [JsonProperty("nsfw")]
        public DiffData<bool?> IsNsfw { get; set; }

        [JsonProperty("bitrate")]
        public DiffData<int?> Bitrate { get; set; }

        public static IAuditLogData Create(IAuditLogData entryData)
        {
            if (entryData is not ChannelUpdateAuditLogData data)
                return null;

            return new AuditChannelUpdated()
            {
                Bitrate = data.Before.Bitrate != data.After.Bitrate ? new DiffData<int?>(data.Before.Bitrate, data.After.Bitrate) : null,
                IsNsfw = data.Before.IsNsfw != data.After.IsNsfw ? new DiffData<bool?>(data.Before.IsNsfw, data.After.IsNsfw) : null,
                Name = data.Before.Name != data.After.Name ? new DiffData<string>(data.Before.Name, data.After.Name) : null,
                SlowModeInterval = data.Before.SlowModeInterval != data.After.SlowModeInterval ? new DiffData<int?>(data.Before.SlowModeInterval, data.After.SlowModeInterval) : null,
                Topic = data.Before.Topic != data.After.Topic ? new DiffData<string>(data.Before.Topic, data.After.Topic) : null
            };
        }
    }
}
