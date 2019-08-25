using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Grillbot.Repository.Entity
{
    [Table("EmoteStatistics")]
    public class EmoteStat
    {
        [Key]
        [Column]
        [Required]
        [StringLength(255)]
        public string EmoteID { get; set; }

        [Column]
        public long Count { get; set; }

        [Column]
        public DateTime LastOccuredAt { get; set; } = DateTime.MinValue;

        public EmoteStat() { }

        public EmoteStat(string emoteId)
        {
            EmoteID = emoteId;
            IncrementAndUpdate();
        }

        public void IncrementAndUpdate()
        {
            LastOccuredAt = DateTime.Now;
            Count++;
        }

        public string GetFormatedInfo()
        {
            return new StringBuilder()
                .Append("Počet použití: ").AppendLine(Count.ToString())
                .Append("Naposledy použito: ").AppendLine(LastOccuredAt.ToString())
                .ToString();
        }
    }
}
