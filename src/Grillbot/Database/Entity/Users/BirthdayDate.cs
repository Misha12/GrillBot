using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Grillbot.Database.Entity.Users
{
    [Table("BirthdayDates")]
    public class BirthdayDate
    {
        [Key]
        public long UserID { get; set; }

        [ForeignKey("UserID")]
        public DiscordUser User { get; set; }

        [Required]
        public DateTime Date { get; set; }

        public bool AcceptAge { get; set; }
    }
}
