using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace Grillbot.Models.Audit
{
    public class RoleEditInfo
    {
        [JsonProperty("id")]
        public ulong RoleId { get; set; }

        [JsonIgnore]
        public IRole Role { get; set; }

        [JsonProperty("add")]
        public bool Added { get; set; }

        public RoleEditInfo() { }
        public RoleEditInfo(MemberRoleEditInfo info)
        {
            RoleId = info.RoleId;
            Added = info.Added;
        }

        public RoleEditInfo GetFilledModel(SocketGuild guild)
        {
            Role = guild.GetRole(RoleId);
            return this;
        }

        public override int GetHashCode()
        {
            return (RoleId.ToString() + Added.ToString()).GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is not RoleEditInfo roleEdit)
                return false;

            return roleEdit.RoleId == RoleId && roleEdit.Added == Added;
        }
    }
}
