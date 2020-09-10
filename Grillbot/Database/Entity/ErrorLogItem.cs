using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Grillbot.Database.Entity
{
    [Table("Errors")]
    public class ErrorLogItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ID { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string Data { get; set; }
    }
}
