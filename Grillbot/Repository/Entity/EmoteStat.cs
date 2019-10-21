﻿using Grillbot.Extensions;
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

        [Column]
        [Required]
        public bool IsUnicode { get; set; }

        public EmoteStat() { }

        public EmoteStat(string emoteId, bool isUnicode)
        {
            EmoteID = emoteId;
            IsUnicode = isUnicode;
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
                .Append("Počet použití: ").AppendLine(Count.ToString())
                .Append("Naposledy použito: ").AppendLine(LastOccuredAt.ToLocaleDatetime())
                .ToString();
        }

        public string GetRealId() => IsUnicode ? Encoding.Unicode.GetString(Convert.FromBase64String(EmoteID)) : EmoteID;
    }
}
