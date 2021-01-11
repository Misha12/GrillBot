using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Grillbot.Extensions.Discord;
using Newtonsoft.Json;

namespace Grillbot.Models.Audit.DiscordAuditLog
{
    public class AuditOverwriteUpdated : IAuditLogData
    {
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

        public static MappedAuditLogItem Create(IAuditLogData entryData)
        {
            if (entryData is not OverwriteUpdateAuditLogData data)
                return null;

            var item = new AuditOverwriteUpdated()
            {
                TargetId = data.OverwriteTargetId,
                TargetType = data.OverwriteType
            };

            if (data.OldPermissions.AllowValue != data.NewPermissions.AllowValue || data.OldPermissions.DenyValue != data.NewPermissions.DenyValue)
            {
                var oldPerms = new OverwritePermissionsValue(data.OldPermissions);
                var newPerms = new OverwritePermissionsValue(data.NewPermissions);

                item.Permissions = new DiffData<OverwritePermissionsValue>(oldPerms, newPerms);
            }

            return new MappedAuditLogItem(data.ChannelId, item);
        }

        public AuditOverwriteUpdated GetFilledModel(SocketGuild guild)
        {
            if (TargetType == PermissionTarget.Role)
                TargetRole = guild.GetRole(TargetId);
            else if (TargetType == PermissionTarget.User)
                TargetUser = guild.GetUserFromGuildAsync(TargetId).Result;

            return this;
        }
    }
}
