using Grillbot.Extensions;
using Grillbot.Helpers;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Grillbot.Database.Entity
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

        [Column]
        [Required]
        public bool IsUnicode { get; set; }

        [Column]
        [Required]
        public string GuildID { get; set; }

        [NotMapped]
        public ulong GuildIDSnowflake
        {
            get => string.IsNullOrEmpty(GuildID) ? 0 : Convert.ToUInt64(GuildID);
            set => GuildID = value.ToString();
        }

        public EmoteStat() { }

        public EmoteStat(string emoteId, bool isUnicode, ulong guildID)
        {
            EmoteID = emoteId;
            IsUnicode = isUnicode;
            GuildIDSnowflake = guildID;
            IncrementAndUpdate();
        }

        public void IncrementAndUpdate()
        {
            LastOccuredAt = DateTime.Now;
            Count++;
        }

        public void Decrement()
        {
            Count--;
        }

        public string GetFormatedInfo()
        {
            return new StringBuilder()
                .Append("Počet použití: ").AppendLine(FormatHelper.FormatWithSpaces(Count))
                .Append("Naposledy použito: ").AppendLine(LastOccuredAt.ToLocaleDatetime())
                .ToString();
        }

        public string GetRealId() => IsUnicode ? Encoding.Unicode.GetString(Convert.FromBase64String(EmoteID)) : EmoteID;
    }
}
