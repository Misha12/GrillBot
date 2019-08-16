using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Grillbot.Repository.Entity
{
    [Table("LoggerAttachmentCache")]
    public class LoggerAttachment
    {
        [Key]
        [Column]
        [Required]
        [StringLength(30)]
        public string AttachmentID { get; set; }

        [Column]
        public string MessageID { get; set; }

        [ForeignKey("MessageID")]
        public LoggerMessage Message { get; set; }

        [Column]
        [StringLength(255)]
        public string UrlLink { get; set; }

        [Column]
        [StringLength(255)]
        public string ProxyUrl { get; set; }

        [NotMapped]
        public ulong SnowflakeAttachmentID
        {
            get => Convert.ToUInt64(AttachmentID);
            set => AttachmentID = value.ToString();
        }

        [NotMapped]
        public ulong SnowflakeMessageID
        {
            get => Convert.ToUInt64(MessageID);
            set => MessageID = value.ToString();
        }
    }
}
