using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Grillbot.Repository.Entity
{
    [Table("LoggerMessageCache")]
    public class LoggerMessage
    {
        [Key]
        [Column]
        [Required]
        [StringLength(30)]
        public string MessageID { get; set; }

        [Column]
        [Required]
        [StringLength(30)]
        public string AuthorID { get; set; }

        [Column]
        [Required]
        [StringLength(30)]
        public string ChannelID { get; set; }

        [Column]
        public string Content { get; set; }

        [Column]
        public DateTime CreatedAt { get; set; }

        [NotMapped]
        public ulong SnowflakeMessageID
        {
            get => Convert.ToUInt64(MessageID);
            set => MessageID = value.ToString();
        }

        [NotMapped]
        public ulong SnowflakeAuthorID
        {
            get => Convert.ToUInt64(AuthorID);
            set => AuthorID = value.ToString();
        }

        [NotMapped]
        public ulong SnowflakeChannelID
        {
            get => Convert.ToUInt64(ChannelID);
            set => ChannelID = value.ToString();
        }

        public ICollection<LoggerAttachment> Attachments { get; set; }

        public LoggerMessage()
        {
            Attachments = new HashSet<LoggerAttachment>();
        }
    }
}