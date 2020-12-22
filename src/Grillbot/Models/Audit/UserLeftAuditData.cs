using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace Grillbot.Models.Audit
{
    public class UserLeftAuditData
    {
        [JsonProperty("cnt")]
        public int MemberCount { get; set; }

        [JsonProperty("ban")]
        public bool IsBan { get; set; }

        [JsonProperty("ban_r")]
        public string BanReason { get; set; }

        [JsonProperty("usr")]
        public AuditUserInfo User { get; set; }

        public UserLeftAuditData() { }

        public UserLeftAuditData(int memberCount, bool isBan, string banReason, AuditUserInfo user)
        {
            MemberCount = memberCount;
            IsBan = isBan;
            BanReason = banReason;
            User = user;
        }

        public UserLeftAuditData(int memberCount, bool isBan, string banReason, IUser user) :
            this(memberCount, isBan, banReason, AuditUserInfo.Create(user))
        {
        }

        public static UserLeftAuditData Create(SocketGuild guild, IUser user, bool isBan, string banReason)
        {
            return new UserLeftAuditData(guild.MemberCount, isBan, banReason, user);
        }
    }
}
