using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Grillbot.Enums;
using Grillbot.Extensions.Discord;
using Newtonsoft.Json;

namespace Grillbot.Models.Audit.DiscordAuditLog
{
    public class AuditOverwriteUpdated : IAuditLogData
    {
        [JsonProperty("ch_id")]
        public ulong ChannelId { get; set; }

        [JsonIgnore]
        public IChannel Channel { get; set; }

        [JsonProperty("target")]
        public ulong TargetId { get; set; }

        [JsonIgnore]
        public IUser TargetUser { get; set; }

        [JsonIgnore]
        public IRole TargetRole { get; set; }

        [JsonProperty("type")]
        public PermissionTarget TargetType { get; set; }

        [JsonProperty("perms")]
        public DiffData<OverwritePermissionsValue> Permissions { get; set; }

        public static IAuditLogData Create(IAuditLogData entryData)
        {
            if (entryData is not OverwriteUpdateAuditLogData data)
                return null;

            var item = new AuditOverwriteUpdated()
            {
                ChannelId = data.ChannelId,
                TargetId = data.OverwriteTargetId,
                TargetType = data.OverwriteType
            };

            if(data.OldPermissions.AllowValue != data.NewPermissions.AllowValue || data.OldPermissions.DenyValue != data.NewPermissions.DenyValue)
            {
                var oldPerms = new OverwritePermissionsValue(data.OldPermissions);
                var newPerms = new OverwritePermissionsValue(data.NewPermissions);

                item.Permissions = new DiffData<OverwritePermissionsValue>(oldPerms, newPerms);
            }

            return item;
        }

        public AuditOverwriteUpdated GetFilledModel(SocketGuild guild)
        {
            Channel = guild.GetChannel(ChannelId);

            if (TargetType == PermissionTarget.Role)
                TargetRole = guild.GetRole(TargetId);
            else if (TargetType == PermissionTarget.User)
                TargetUser = guild.GetUserFromGuildAsync(TargetId).Result;

            return this;
        }

        public static AuditOverwriteUpdated FromJsonIfValid(AuditLogType type, string json)
        {
            return type == AuditLogType.OverwriteUpdated ? JsonConvert.DeserializeObject<AuditOverwriteUpdated>(json) : null;
        }
    }
}
