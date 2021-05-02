using Grillbot.Database.Entity.AuditLog;
using Grillbot.Database.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Grillbot.Database.Entity.Users
{
    [Table("DiscordUsers")]
    public class DiscordUser
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ID { get; set; }

        [StringLength(30)]
        public string UserID { get; set; }

        [NotMapped]
        public ulong UserIDSnowflake
        {
            get => Convert.ToUInt64(UserID);
            set => UserID = value.ToString();
        }

        [StringLength(30)]
        public string GuildID { get; set; }

        [NotMapped]
        public ulong GuildIDSnowflake
        {
            get => Convert.ToUInt64(GuildID);
            set => GuildID = value.ToString();
        }

        public long Points { get; set; }
        public long GivenReactionsCount { get; set; }
        public long ObtainedReactionsCount { get; set; }
        public string WebAdminPassword { get; set; }
        public string ApiToken { get; set; }
        public string UsedInviteCode { get; set; }
        public long Flags { get; set; }
        public int? WebAdminLoginCount { get; set; }
        public DateTime? Birthday { get; set; }
        public int FailedLoginCount { get; set; }
        public DateTime? WebAdminBannedTo { get; set; }

        [ForeignKey("UsedInviteCode")]
        public Invite UsedInvite { get; set; }

        public ISet<UserChannel> Channels { get; set; }
        public ISet<Reminder> Reminders { get; set; }
        public ISet<Invite> CreatedInvites { get; set; }
        public ISet<EmoteStatItem> UsedEmotes { get; set; }
        public Unverify.Unverify Unverify { get; set; }
        public ISet<Unverify.UnverifyLog> OutgoingUnverifyOperations { get; set; }
        public ISet<Unverify.UnverifyLog> IncomingUnverifyOperations { get; set; }
        public ISet<AuditLogItem> AuditLogs { get; set; }

        [NotMapped]
        public bool IsBotAdmin
        {
            get => (Flags & (long)UserFlags.BotAdmin) != 0;
            set
            {
                if (value)
                    Flags |= (long)UserFlags.BotAdmin;
                else
                    Flags &= ~(long)UserFlags.BotAdmin;
            }
        }

        public DiscordUser()
        {
            Channels = new HashSet<UserChannel>();
            Reminders = new HashSet<Reminder>();
            CreatedInvites = new HashSet<Invite>();
            UsedEmotes = new HashSet<EmoteStatItem>();
            OutgoingUnverifyOperations = new HashSet<Unverify.UnverifyLog>();
            IncomingUnverifyOperations = new HashSet<Unverify.UnverifyLog>();
            AuditLogs = new HashSet<AuditLogItem>();
        }
    }
}
