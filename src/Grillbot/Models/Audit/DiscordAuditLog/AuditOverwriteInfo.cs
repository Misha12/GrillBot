using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Grillbot.Extensions.Discord;
using Newtonsoft.Json;

namespace Grillbot.Models.Audit.DiscordAuditLog
{
    public class AuditOverwriteInfo : IAuditLogData
    {
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

        public AuditOverwriteInfo(Overwrite overwrite)
        {
            TargetId = overwrite.TargetId;
            PermissionTarget = overwrite.TargetType;
            Permissions = new OverwritePermissionsValue(overwrite.Permissions);
        }

        public AuditOverwriteInfo(OverwriteCreateAuditLogData data) : this(data.Overwrite) { }
        public AuditOverwriteInfo(OverwriteDeleteAuditLogData data) : this(data.Overwrite) { }

        public static MappedAuditLogItem Create(IAuditLogData entryData)
        {
            if (entryData is OverwriteCreateAuditLogData createData)
                return new MappedAuditLogItem(createData.ChannelId, new AuditOverwriteInfo(createData));
            else if (entryData is OverwriteDeleteAuditLogData deleteData)
                return new MappedAuditLogItem(deleteData.ChannelId, new AuditOverwriteInfo(deleteData));
            else
                return null;
        }

        public AuditOverwriteInfo GetFilledModel(SocketGuild guild)
        {
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
    }
}
