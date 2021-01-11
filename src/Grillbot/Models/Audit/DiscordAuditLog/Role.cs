using Discord;
using Discord.Rest;
using Newtonsoft.Json;

namespace Grillbot.Models.Audit.DiscordAuditLog
{
    public class Role : IAuditLogData
    {
        [JsonProperty("id")]
        public ulong RoleId { get; set; }

        [JsonProperty("color")]
        public string Color { get; set; }

        [JsonProperty("mention")]
        public bool Mentionable { get; set; }

        [JsonProperty("hoist")]
        public bool IsHoisted { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("perms")]
        public ulong PermsValue { get; set; }

        [JsonIgnore]
        public GuildPermissions GuildPerms => new GuildPermissions(PermsValue);

        public Role() { }

        public Role(ulong roleId, Discord.Rest.RoleEditInfo info)
        {
            RoleId = roleId;
            Color = info.Color?.ToString();
            Mentionable = info.Mentionable ?? false;
            IsHoisted = info.Hoist ?? false;
            Name = info.Name;
            PermsValue = info.Permissions?.RawValue ?? 0;
        }

        public Role(RoleCreateAuditLogData data) : this(data.RoleId, data.Properties) { }
        public Role(RoleDeleteAuditLogData data) : this(data.RoleId, data.Properties) { }

        public static MappedAuditLogItem Create(IAuditLogData entryData)
        {
            if (entryData is RoleCreateAuditLogData createData)
                return new MappedAuditLogItem(null, new Role(createData));
            else if (entryData is RoleDeleteAuditLogData deleteData)
                return new MappedAuditLogItem(null, new Role(deleteData));
            else
                return null;
        }
    }
}
