using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Grillbot.Database.Entity.MethodConfig
{
    [Table("MethodsConfig")]
    public class MethodsConfig
    {
        [Key]
        [Column]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Column]
        [Required]
        [StringLength(30)]
        public string GuildID { get; set; }

        [NotMapped]
        public ulong GuildIDSnowflake => Convert.ToUInt64(GuildID);

        [Column]
        [Required]
        [StringLength(100)]
        public string Group { get; set; }

        [Column]
        [Required]
        [StringLength(100)]
        public string Command { get; set; }

        [Column]
        [Required]
        public string ConfigData { get; set; }

        public TData GetData<TData>() => string.IsNullOrEmpty(ConfigData) ? default : JsonConvert.DeserializeObject<TData>(ConfigData);

        [Column]
        public bool PMAllowed { get; set; }

        [Column]
        public bool OnlyAdmins { get; set; }

        public ISet<MethodPerm> Permissions { get; set; }

        public MethodsConfig()
        {
            Permissions = new HashSet<MethodPerm>();
        }
    }
}
