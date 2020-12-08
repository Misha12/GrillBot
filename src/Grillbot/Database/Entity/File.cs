using Grillbot.Database.Entity.AuditLog;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Grillbot.Database.Entity
{
    public class File
    {
        [Key]
        public string Filename { get; set; }

        public byte[] Content { get; set; }

        public long? AuditLogItemId { get; set; }

        [ForeignKey("AuditLogItemId")]
        public AuditLogItem AuditLogItem { get; set; }
    }
}
