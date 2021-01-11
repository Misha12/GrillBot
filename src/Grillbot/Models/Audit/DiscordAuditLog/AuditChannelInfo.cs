using Discord;
using Discord.Rest;
using Newtonsoft.Json;

namespace Grillbot.Models.Audit
{
    public class AuditChannelInfo : IAuditLogData
    {
        [JsonIgnore]
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

        public AuditChannelInfo(string name, ChannelType type, bool? isNsfw, int? slowmode, int? bitrate)
        {
            ChannelName = name;
            ChannelType = type;
            IsNsfw = isNsfw;
            SlowModeInterval = slowmode;
            Bitrate = bitrate;
        }

        public AuditChannelInfo(ChannelCreateAuditLogData data) : this(data.ChannelName, data.ChannelType, data.IsNsfw, data.SlowModeInterval, data.Bitrate) { }
        public AuditChannelInfo(ChannelDeleteAuditLogData data) : this(data.ChannelName, data.ChannelType, data.IsNsfw, data.SlowModeInterval, data.Bitrate) { }

        public static MappedAuditLogItem Create(IAuditLogData entryData)
        {
            if (entryData is ChannelCreateAuditLogData data)
                return new MappedAuditLogItem(data.ChannelId, new AuditChannelInfo(data));
            else if (entryData is ChannelDeleteAuditLogData deleteData)
                return new MappedAuditLogItem(deleteData.ChannelId, new AuditChannelInfo(deleteData));
            else
                return null;
        }

        public AuditChannelInfo GetFilledModel(ulong channelId)
        {
            ChannelId = channelId;
            return this;
        }
    }
}
