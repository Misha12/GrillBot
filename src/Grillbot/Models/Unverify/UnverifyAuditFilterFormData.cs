using Grillbot.Database.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace Grillbot.Models.Unverify
{
    public class UnverifyAuditFilterFormData
    {
        public ulong GuildID { get; set; }
        public string FromUserQuery { get; set; }
        public string ToUserQuery { get; set; }
        public UnverifyLogOperation? Operation { get; set; }
        public DateTime? DateTimeFrom { get; set; }
        public DateTime? DateTimeTo { get; set; }
        public bool OrderAsc { get; set; }

        [Range(0, double.MaxValue)]
        public int Page { get; set; } = 1;
    }
}
