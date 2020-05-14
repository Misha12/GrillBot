using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Grillbot.Database.Entity.Users
{
    [Table("UserChannels")]
    public class UserChannel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ID { get; set; }

        [StringLength(30)]
        public string ChannelID { get; set; }

        [NotMapped]
        public ulong ChannelIDSnowflake
        {
            get => Convert.ToUInt64(ChannelID);
            set => ChannelID = value.ToString();
        }

        [StringLength(30)]
        public string DiscordUserID { get; set; }

        [NotMapped]
        public ulong DiscordUserIDSnowflake
        {
            get => Convert.ToUInt64(DiscordUserID);
            set => DiscordUserID = value.ToString();
        }

        public long UserID { get; set; }

        [ForeignKey("UserID")]
        public DiscordUser User { get; set; }

        public string GuildID { get; set; }

        [NotMapped]
        public ulong GuildIDSnowflake
        {
            get => Convert.ToUInt64(GuildID);
            set => GuildID = value.ToString();
        }

        public long Count { get; set; }
        public DateTime LastMessageAt { get; set; }
    }
}
