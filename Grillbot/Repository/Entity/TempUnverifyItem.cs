using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Grillbot.Repository.Entity
{
    [Table("TempUnverify")]
    public class TempUnverifyItem
    {
        [Key]
        [Column]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Column]
        [Required]
        [StringLength(30)]
        public string UserID { get; set; }

        [Column]
        [Required]
        public long TimeFor { get; set; }

        [Column]
        [Required]
        public DateTime StartAt { get; set; } = DateTime.Now;

        public List<string> RolesToReturn { get; set; }

        public DateTime GetEndDatetime() => StartAt.AddSeconds(TimeFor);

        public TempUnverifyItem()
        {
            RolesToReturn = new List<string>();
        }
    }
}