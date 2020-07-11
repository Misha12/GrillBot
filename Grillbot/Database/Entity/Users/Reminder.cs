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
        public DiscordUser User { get;set; }

        public long? FromUserID { get; set; }

        [ForeignKey("FromUserID")]
        public DiscordUser FromUser { get; set; }

        public DateTime At { get; set; }

        public string Message { get; set; }
    }
}