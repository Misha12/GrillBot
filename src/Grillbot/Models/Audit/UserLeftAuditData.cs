using Discord;
using Discord.WebSocket;
using Grillbot.Extensions;
using System;
using System.Collections.Immutable;

namespace Grillbot.Models.Audit
{
    public class UserLeftAuditData
    {
        public int MemberCount { get; set; }
        public bool IsBan { get; set; }
        public string BanReason { get; set; }
        public AuditUserInfo User { get; set; }

        public UserLeftAuditData() { }

        public UserLeftAuditData(int memberCount, bool isBan, string banReason, AuditUserInfo user)
        {
            MemberCount = memberCount;
            IsBan = isBan;
            BanReason = banReason;
            User = user;
        }

        public UserLeftAuditData(int memberCount, bool isBan, string banReason, IUser user) : this(memberCount, isBan, banReason, AuditUserInfo.Create(user))
        {
        }

        public static UserLeftAuditData Create(SocketGuild guild, IUser user, bool isBan, string banReason)
        {
            return new UserLeftAuditData(guild.MemberCount, isBan, banReason, user);
        }

        public static UserLeftAuditData Create(ImmutableArray<EmbedField> fields)
        {
            var userIdentification = fields[0].Value.Split("#");
            var user = new AuditUserInfo()
            {
                Discriminator = userIdentification[1],
                Username = userIdentification[0]
            };

            var memberCount = Convert.ToInt32(fields[1].Value);
            var isBan = fields.Length >= 3 && fields[2].Value.TranslateCzToBool();
            var banReason = isBan && fields.Length >= 4 ? fields[3].Value : null;

            return new UserLeftAuditData(memberCount, isBan, banReason, user);
        }
    }
}
