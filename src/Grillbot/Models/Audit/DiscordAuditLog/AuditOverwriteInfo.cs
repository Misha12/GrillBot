using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Grillbot.Enums;
using Grillbot.Extensions.Discord;
using Newtonsoft.Json;

namespace Grillbot.Models.Audit.DiscordAuditLog
{
    public class AuditOverwriteInfo : IAuditLogData
    {
        [JsonProperty("ch_id")]
        public ulong ChannelId { get; set; }

        [JsonIgnore]
        public IChannel Channel { get; set; }

        [JsonProperty("type")]
        public PermissionTarget PermissionTarget { get; set; }

        [JsonProperty("target")]
        public ulong TargetId { get; set; }

        [JsonIgnore]
        public IUser TargetUser { get; set; }

        [JsonIgnore]
        public IRole TargetRole { get; set; }

        [JsonProperty("perms")]
        public OverwritePermissionsValue Permissions { get; set; }

        public AuditOverwriteInfo() { }

        public AuditOverwriteInfo(ulong channelId, Overwrite overwrite)
        {
            ChannelId = channelId;
            TargetId = overwrite.TargetId;
            PermissionTarget = overwrite.TargetType;
            Permissions = new OverwritePermissionsValue(overwrite.Permissions);
        }

        public AuditOverwriteInfo(OverwriteCreateAuditLogData data) : this(data.ChannelId, data.Overwrite) { }
        public AuditOverwriteInfo(OverwriteDeleteAuditLogData data) : this(data.ChannelId, data.Overwrite) { }

        public static IAuditLogData Create(IAuditLogData entryData)
        {
            if (entryData is OverwriteCreateAuditLogData createData)
                return new AuditOverwriteInfo(createData);
            else if (entryData is OverwriteDeleteAuditLogData deleteData)
                return new AuditOverwriteInfo(deleteData);
            else
                return null;
        }

        public AuditOverwriteInfo GetFilledModel(SocketGuild guild)
        {
            Channel = guild.GetChannel(ChannelId);

            switch(PermissionTarget)
            {
                case PermissionTarget.Role:
                    TargetRole = guild.GetRole(TargetId);
                    break;
                case PermissionTarget.User:
                    TargetUser = guild.GetUserFromGuildAsync(TargetId).Result;
                    break;
            }

            return this;
        }

        public static AuditOverwriteInfo FromJsonIfValid(AuditLogType type, string json)
        {
            if (type != AuditLogType.OverwriteCreated && type != AuditLogType.OverwriteDeleted)
                return null;

            return JsonConvert.DeserializeObject<AuditOverwriteInfo>(json);
        }
    }
}
