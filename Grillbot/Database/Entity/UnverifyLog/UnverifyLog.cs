using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

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

        [Column]
        [StringLength(30)]
        public string GuildID { get; set; }

        [Column]
        public DateTime DateTime { get; set; } = DateTime.Now;

        [Column]
        public string Data { get; set; }

        public T GetData<T>() => JsonConvert.DeserializeObject<T>(Data);
    }
}
