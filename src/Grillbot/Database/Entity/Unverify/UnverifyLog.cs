using Grillbot.Database.Entity.Users;
using Grillbot.Database.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Grillbot.Database.Entity.Unverify
{
    [Table("UnverifyLogs")]
    public class UnverifyLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ID { get; set; }

        public UnverifyLogOperation Operation { get; set; }

        public long FromUserID { get; set; }

        [ForeignKey("FromUserID")]
        public DiscordUser FromUser { get; set; }

        public long ToUserID { get; set; }

        [ForeignKey("ToUserID")]
        public DiscordUser ToUser { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string JsonData { get; set; }

        public Unverify Unverify { get; set; }

        #region NotMapped objects

        [NotMapped]
        public JObject Json
        {
            get => string.IsNullOrEmpty(JsonData) ? new JObject() : JObject.Parse(JsonData);
            set => JsonData = value?.ToString(Formatting.None);
        }

        #endregion
    }
}
