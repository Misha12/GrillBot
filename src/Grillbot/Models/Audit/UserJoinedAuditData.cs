using Discord.WebSocket;
using Grillbot.Models.Users;
using Newtonsoft.Json;

namespace Grillbot.Models.Audit
{
    public class UserJoinedAuditData
    {
        [JsonProperty("cnt")]
        public int MemberCount { get; set; }

        [JsonIgnore]
        public DiscordUser User { get; set; }

        public UserJoinedAuditData() { }

        public UserJoinedAuditData(int memberCount)
        {
            MemberCount = memberCount;
        }

        public static UserJoinedAuditData Create(SocketGuild guild)
        {
            return Create(guild.MemberCount);
        }

        public static UserJoinedAuditData Create(int memberCount)
        {
            return new UserJoinedAuditData(memberCount);
        }

        public UserJoinedAuditData GetFilledModel(DiscordUser user)
        {
            User = user;
            return this;
        }
    }
}
