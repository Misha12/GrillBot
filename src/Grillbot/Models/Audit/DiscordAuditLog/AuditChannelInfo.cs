using Discord;
using Discord.Rest;
using Newtonsoft.Json;

namespace Grillbot.Models.Audit
{
    public class AuditChannelInfo : IAuditLogData
    {
        [JsonProperty("id")]
        public ulong ChannelId { get; set; }

        [JsonProperty("name")]
        public string ChannelName { get; set; }

        [JsonProperty("type")]
        public ChannelType ChannelType { get; set; }

        [JsonProperty("nsfw")]
        public bool? IsNsfw { get; set; }

        [JsonProperty("bitrate")]
        public int? Bitrate { get; set; }

        [JsonProperty("slowmode")]
        public int? SlowModeInterval { get; set; }

        public AuditChannelInfo() { }

        public AuditChannelInfo(ulong id, string name, ChannelType type, bool? isNsfw, int? slowmode, int? bitrate)
        {
            ChannelId = id;
            ChannelName = name;
            ChannelType = type;
            IsNsfw = isNsfw;
            SlowModeInterval = slowmode;
            Bitrate = bitrate;
        }

        public AuditChannelInfo(ChannelCreateAuditLogData data) : this(data.ChannelId, data.ChannelName, data.ChannelType, data.IsNsfw, data.SlowModeInterval, data.Bitrate) { }
        public AuditChannelInfo(ChannelDeleteAuditLogData data) : this(data.ChannelId, data.ChannelName, data.ChannelType, data.IsNsfw, data.SlowModeInterval, data.Bitrate) { }

        public static IAuditLogData Create(IAuditLogData entryData)
        {
            if (entryData is ChannelCreateAuditLogData data)
                return new AuditChannelInfo(data);
            else if (entryData is ChannelDeleteAuditLogData deleteData)
                return new AuditChannelInfo(deleteData);
            else
                return null;
        }
    }
}
