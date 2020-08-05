using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Grillbot.Database.Entity.Users
{
    [Table("EmoteStats")]
    public class EmoteStatItem
    {
        [Key]
        [StringLength(150)]
        public string EmoteID { get; set; }

        public long UserID { get; set; }

        [ForeignKey("UserID")]
        public DiscordUser User { get; set; }

        public long UseCount { get; set; }

        public DateTime LastOccuredAt { get; set; } = DateTime.Now;

        public DateTime FirstOccuredAt { get; set; } = DateTime.Now;

        public bool IsUnicode { get; set; }

        [NotMapped]
        public string RealID
        {
            get => IsUnicode ? Encoding.Unicode.GetString(Convert.FromBase64String(EmoteID)) : EmoteID;
        }
    }
}
