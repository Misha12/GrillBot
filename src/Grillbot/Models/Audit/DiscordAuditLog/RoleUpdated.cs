using Discord;
using Discord.Rest;
using Grillbot.Enums;
using Newtonsoft.Json;

namespace Grillbot.Models.Audit.DiscordAuditLog
{
    public class RoleUpdated : IAuditLogData
    {
        [JsonProperty("id")]
        public ulong RoleId { get; set; }

        [JsonProperty("color")]
        public DiffData<string> Color { get; set; }

        [JsonProperty("mention")]
        public DiffData<bool> Mentionable { get; set; }

        [JsonProperty("hoist")]
        public DiffData<bool> IsHoisted { get; set; }

        [JsonProperty("name")]
        public DiffData<string> Name { get; set; }

        [JsonProperty("perms")]
        public DiffData<ulong> PermsValue { get; set; }

        [JsonIgnore]
        public DiffData<GuildPermissions> GuildPerms
        {
            get => PermsValue != null ? new DiffData<GuildPermissions>(new GuildPermissions(PermsValue.Before), new GuildPermissions(PermsValue.After)) : null;
        }

        public static IAuditLogData Create(IAuditLogData entryData)
        {
            if (entryData is not RoleUpdateAuditLogData data)
                return null;

            var item = new RoleUpdated()
            {
                RoleId = data.RoleId,
                Name = data.Before.Name != data.After.Name ? new DiffData<string>(data.Before.Name, data.After.Name) : null,
                Mentionable = data.Before.Mentionable != data.After.Mentionable ? new DiffData<bool>(data.Before.Mentionable ?? false, data.After.Mentionable ?? false) : null,
                IsHoisted = data.Before.Hoist != data.After.Hoist ? new DiffData<bool>(data.Before.Hoist ?? false, data.After.Hoist ?? false) : null,
                Color = data.Before.Color != data.After.Color ? new DiffData<string>(data.Before.Color?.ToString(), data.After.Color?.ToString()) : null
            };

            if (data.Before.Permissions?.RawValue != data.After.Permissions?.RawValue)
            {
                var oldPerms = data.Before.Permissions?.RawValue ?? 0;
                var newPerms = data.After.Permissions?.RawValue ?? 0;

                item.PermsValue = new DiffData<ulong>(oldPerms, newPerms);
            }

            return item;
        }

        public static RoleUpdated FromJsonIfValid(AuditLogType type, string json)
        {
            return type == AuditLogType.RoleUpdated ? JsonConvert.DeserializeObject<RoleUpdated>(json) : null;
        }
    }
}
