using Discord.WebSocket;
using Grillbot.Models.Users;
using Newtonsoft.Json;

namespace Grillbot.Models.Audit
{
    public class UserJoinedAuditData
    {
        public int MemberCount { get; set; }

        [JsonIgnore]
        public DiscordUser User { get; set; }

        public static UserJoinedAuditData Create(SocketGuild guild)
        {
            return new UserJoinedAuditData()
            {
                MemberCount = guild.MemberCount
            };
        }

        public UserJoinedAuditData GetFilledModel(DiscordUser user)
        {
            User = user;
            return this;
        }
    }
}
