using Grillbot.Database.Entity.Users;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Grillbot.Database.Entity.Math
{
    [Table("MathAuditLog")]
    public class MathAuditLogItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        public string Expression { get; set; }

        public long UserID { get; set; }

        [ForeignKey("UserID")]
        public DiscordUser User { get; set; }

        [StringLength(30)]
        public string ChannelID { get; set; }

        [NotMapped]
        public ulong ChannelIDSnowflake
        {
            get => Convert.ToUInt64(ChannelID);
            set => ChannelID = value.ToString();
        }

        public string UnitInfo { get; set; }
        public string Result { get; set; }
        public DateTime DateTime { get; set; }
    }
}
