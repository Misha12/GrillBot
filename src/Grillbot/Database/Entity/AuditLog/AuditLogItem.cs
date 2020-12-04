using Grillbot.Database.Entity.Users;
using Grillbot.Enums;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Grillbot.Database.Entity.AuditLog
{
    public class AuditLogItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public long? UserId { get; set; }

        public DiscordUser User { get; set; }

        [StringLength(30)]
        [Required]
        public string GuildId { get; set; }

        [NotMapped]
        public ulong GuildIdSnowflake
        {
            get => Convert.ToUInt64(GuildId);
            set => GuildId = value.ToString();
        }

        public string DcAuditLogId { get; set; }

        [NotMapped]
        public ulong? DcAuditLogIdSnowflake
        {
            get => string.IsNullOrEmpty(DcAuditLogId) ? null : Convert.ToUInt64(DcAuditLogId);
            set => DcAuditLogId = value?.ToString();
        }

        public string JsonData { get; set; }

        [NotMapped]
        public JObject Data
        {
            get => string.IsNullOrEmpty(JsonData) ? null : JObject.Parse(JsonData);
            set => JsonData = value?.ToString(Newtonsoft.Json.Formatting.None);
        }

        public AuditLogType Type { get; set; }
    }
}
