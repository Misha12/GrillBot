using System.Collections.Generic;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Grillbot.Database.Entity
{
    [Table("CommandLog")]
    public class CommandLog
    {
        [Key]
        [Column]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ID { get; set; }

        [Column]
        [StringLength(100)]
        public string Group { get; set; }

        [Column]
        [Required]
        [StringLength(100)]
        public string Command { get; set; }

        [Column]
        [Required]
        [StringLength(255)]
        public string UserID { get; set; }

        [NotMapped]
        public ulong UserIDSnowflake
        {
            get => Convert.ToUInt64(UserID);
            set => UserID = value.ToString();
        }

        [Column]
        [Required]
        public DateTime CalledAt { get; set; } = DateTime.Now;

        [Column]
        [Required]
        public string FullCommand { get; set; }

        [Column]
        [StringLength(100)]
        public string GuildID { get; set; }

        [NotMapped]
        public ulong? GuildIDSnowflake
        {
            get => string.IsNullOrEmpty(GuildID) ? (ulong?)null : Convert.ToUInt64(GuildID);
            set => GuildID = value?.ToString();
        }

        [Column]
        [StringLength(100)]
        public string ChannelID { get; set; }

        [NotMapped]
        public ulong ChannelIDSnowflake
        {
            get => Convert.ToUInt64(ChannelID);
            set => ChannelID = value.ToString();
        }
    }
}