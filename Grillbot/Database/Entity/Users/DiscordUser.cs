﻿using Grillbot.Database.Entity.Math;
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

        public ISet<UserChannel> Channels { get; set; }
        public BirthdayDate Birthday { get; set; }
        public ISet<MathAuditLogItem> MathAudit { get; set; }
        public StatisticItem Statistics { get; set; }
        public ISet<Reminder> Reminders { get; set; } 

        public DiscordUser()
        {
            Channels = new HashSet<UserChannel>();
            MathAudit = new HashSet<MathAuditLogItem>();
            Reminders = new HashSet<Reminder>();
        }
    }
}
