using Discord;
using Discord.WebSocket;

namespace Grillbot.Models.Audit
{
    public class UserLeftAuditData
    {
        public int MemberCount { get; set; }
        public bool IsBan { get; set; }
        public string BanReason { get; set; }
        public AuditUserInfo User { get; set; }

        public static UserLeftAuditData CreateDbItem(SocketGuild guild, IUser user, bool isBan, string banReason)
        {
            return new UserLeftAuditData()
            {
                BanReason = banReason,
                IsBan = isBan,
                MemberCount = guild.MemberCount,
                User = AuditUserInfo.Create(user)
            };
        }
    }
}
