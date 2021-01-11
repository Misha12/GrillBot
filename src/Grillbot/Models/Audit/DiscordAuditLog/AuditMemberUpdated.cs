using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Grillbot.Extensions.Discord;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Models.Audit.DiscordAuditLog
{
    public class AuditMemberUpdated : IAuditLogData
    {
        [JsonProperty("id")]
        public ulong UserId { get; set; }

        [JsonIgnore]
        public IUser User { get; set; }

        [JsonProperty("nick")]
        public DiffData<string> Nickname { get; set; }

        [JsonProperty("deaf")]
        public DiffData<bool?> Deaf { get; set; }

        [JsonProperty("mute")]
        public DiffData<bool?> Mute { get; set; }

        [JsonProperty("roles")]
        public List<RoleEditInfo> Roles { get; set; }

        public static MappedAuditLogItem Create(IAuditLogData entryData)
        {
            var item = new AuditMemberUpdated();

            if (entryData is MemberUpdateAuditLogData data)
            {
                item.Deaf = data.Before.Deaf != data.After.Deaf ? new DiffData<bool?>(data.Before.Deaf, data.After.Deaf) : null;
                item.Mute = data.Before.Mute != data.After.Mute ? new DiffData<bool?>(data.Before.Mute, data.After.Mute) : null;
                item.Nickname = data.Before.Nickname != data.After.Nickname ? new DiffData<string>(data.Before.Nickname, data.After.Nickname) : null;
                item.UserId = data.Target.Id;

                return new MappedAuditLogItem(null, item);
            }
            else if (entryData is MemberRoleAuditLogData roleData && roleData.Roles.Count > 0)
            {
                item.UserId = roleData.Target.Id;
                item.Roles = roleData.Roles.Select(o => new RoleEditInfo(o)).ToList();

                return new MappedAuditLogItem(null, item);
            }
            else
            {
                return null;
            }
        }

        public async Task<AuditMemberUpdated> GetFilledModelAsync(SocketGuild guild)
        {
            User = await guild.GetUserFromGuildAsync(UserId);

            if (Roles != null)
            {
                foreach (var role in Roles)
                {
                    role.Role = guild.GetRole(role.RoleId);
                }
            }

            return this;
        }
    }
}
