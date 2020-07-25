using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;

namespace Grillbot.Database.Entity.Users
{
    [Table("Reminders")]
    public class Reminder
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long RemindID { get; set; }

        public long UserID { get; set; }

        [ForeignKey("UserID")]
        public DiscordUser User { get; set; }

        public long? FromUserID { get; set; }

        [ForeignKey("FromUserID")]
        public DiscordUser FromUser { get; set; }

        public DateTime At { get; set; }

        public string Message { get; set; }

        public int PostponeCounter { get; set; }

        [StringLength(30)]
        public string RemindMessageID { get; set; }

        [NotMapped]
        public ulong? RemindMessageIDSnowflake
        {
            get => string.IsNullOrEmpty(RemindMessageID) ? (ulong?)null : Convert.ToUInt64(RemindMessageID);
            set => RemindMessageID = value?.ToString();
        }

        [StringLength(30)]
        [Required]
        public string OriginalMessageID { get; set; }

        [NotMapped]
        public ulong OriginalMessageIDSnowflake
        {
            get => Convert.ToUInt64(OriginalMessageID);
            set => OriginalMessageID = value.ToString();
        }
    }
}