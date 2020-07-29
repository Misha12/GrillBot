using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Grillbot.Database.Entity.Users
{
    [Table("Invites")]
    public class Invite
    {
        [Key]
        [StringLength(20)]
        public string Code { get; set; }

        [Required]
        [StringLength(30)]
        public string ChannelId { get; set; }

        [NotMapped]
        public ulong ChannelIdSnowflake
        {
            get => Convert.ToUInt64(ChannelId);
            set => ChannelId = value.ToString();
        }

        public DateTime? CreatedAt { get; set; }

        public long CreatorId { get; set; }

        [ForeignKey("CreatorId")]
        public DiscordUser Creator { get; set; }

        public ISet<DiscordUser> UsedUsers { get; set; }

        public Invite()
        {
            UsedUsers = new HashSet<DiscordUser>();
        }
    }
}
