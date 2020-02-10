using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Grillbot.Database.Entity.MethodConfig
{
    [Table("MethodPerms")]
    public class MethodPerm
    {
        [Key]
        [Column]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PermID { get; set; }

        [Column]
        [Required]
        public int MethodID { get; set; }

        [ForeignKey("MethodID")]
        public MethodsConfig Method { get; set; }

        [Column]
        [Required]
        [StringLength(30)]
        public string DiscordID { get; set; }

        [NotMapped]
        public ulong DiscordIDSnowflake => Convert.ToUInt64(DiscordID);

        [Column]
        [Required]
        public PermType PermType { get; set; }

        [Column]
        [Required]
        public AllowType AllowType { get; set; }
    }
}
