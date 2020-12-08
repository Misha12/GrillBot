using Grillbot.Database.Entity.Users;
using Grillbot.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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

        public TData GetData<TData>()
        {
            var settings = new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore
            };

            if (string.IsNullOrEmpty(JsonData))
                return default;
            else
                return JsonConvert.DeserializeObject<TData>(JsonData, settings);
        }

        public void SetData(object data)
        {
            var settings = new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                Formatting = Formatting.None
            };

            JsonData = JsonConvert.SerializeObject(data, settings);
        }

        public AuditLogType Type { get; set; }
        public ISet<File> Files { get; set; }

        public AuditLogItem()
        {
            Files = new HashSet<File>();
        }
    }
}
