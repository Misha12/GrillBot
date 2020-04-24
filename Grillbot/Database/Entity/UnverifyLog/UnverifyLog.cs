using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Grillbot.Database.Entity.UnverifyLog
{
    [Table("UnverifyLog")]
    public class UnverifyLog
    {
        [Key]
        [Column]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Column]
        public UnverifyLogOperation Operation { get; set; }

        [Column]
        [StringLength(30)]
        public string FromUserID { get; set; }

        [NotMapped]
        public ulong FromUserIDSnowflake
        {
            get => Convert.ToUInt64(FromUserID);
            set => FromUserID = value.ToString();
        }

        [Column]
        [StringLength(30)]
        public string GuildID { get; set; }

        [NotMapped]
        public ulong GuildIDSnowflake
        {
            get => Convert.ToUInt64(GuildID);
            set => GuildID = value.ToString();
        }

        [Column]
        public DateTime DateTime { get; set; } = DateTime.Now;

        [Column]
        public string Data { get; set; }

        [Column]
        public string DestUserID { get; set; }

        [NotMapped]
        public ulong DestUserIDSnowflake
        {
            get => Convert.ToUInt64(DestUserID);
            set => DestUserID = value.ToString();
        }

        public T GetData<T>() => JsonConvert.DeserializeObject<T>(Data);
    }
}
