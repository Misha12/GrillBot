using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Grillbot.Enums;
using Grillbot.Extensions.Discord;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

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

        public static IAuditLogData Create(IAuditLogData entryData)
        {
            var item = new AuditMemberUpdated();

            if (entryData is MemberUpdateAuditLogData data)
            {
                item.Deaf = data.Before.Deaf != data.After.Deaf ? new DiffData<bool?>(data.Before.Deaf, data.After.Deaf) : null;
                item.Mute = data.Before.Mute != data.After.Mute ? new DiffData<bool?>(data.Before.Mute, data.After.Mute) : null;
                item.Nickname = data.Before.Nickname != data.After.Nickname ? new DiffData<string>(data.Before.Nickname, data.After.Nickname) : null;
                item.UserId = data.Target.Id;

                return item;
            }
            else if (entryData is MemberRoleAuditLogData roleData && roleData.Roles.Count > 0)
            {
                item.UserId = roleData.Target.Id;
                item.Roles = roleData.Roles.Select(o => new RoleEditInfo(o)).ToList();

                return item;
            }
            else
            {
                return null;
            }
        }

        public AuditMemberUpdated GetFilledModel(SocketGuild guild)
        {
            User = guild.GetUserFromGuildAsync(UserId).Result;

            if(Roles != null)
            {
                foreach(var role in Roles)
                {
                    role.Role = guild.GetRole(role.RoleId);
                }
            }

            return this;
        }

        public static AuditMemberUpdated FromJsonIfValid(AuditLogType type, string json)
        {
            if (type != AuditLogType.MemberRoleUpdated && type != AuditLogType.MemberUpdated)
                return null;

            return JsonConvert.DeserializeObject<AuditMemberUpdated>(json);
        }
    }
}
