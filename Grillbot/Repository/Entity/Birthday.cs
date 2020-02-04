using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Grillbot.Repository.Entity
{
    [Table("Birthdays")]
    public class Birthday
    {
        [Key]
        [Column]
        [Required]
        public string ID { get; set; }

        [NotMapped]
        public ulong IDSnowflake
        {
            get => Convert.ToUInt64(ID);
            set => ID = value.ToString();
        }

        [Column]
        [Required]
        public DateTime Date { get; set; }

        [Column]
        [Required]
        public bool AcceptAge { get; set; }

        [Column]
        [Required]
        public string ChannelID { get; set; }

        [NotMapped]
        public ulong ChannelIDSnowflake
        {
            get => Convert.ToUInt64(ChannelID);
            set => ChannelID = value.ToString();
        }

        [Column]
        [Required]
        public string GuildID { get; set; }

        [NotMapped]
        public ulong GuildIDSnowflake
        {
            get => Convert.ToUInt64(GuildID);
            set => GuildID = value.ToString();
        }
    }
}
