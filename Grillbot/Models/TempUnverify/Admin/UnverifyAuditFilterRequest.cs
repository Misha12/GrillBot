using Grillbot.Database.Entity.UnverifyLog;
using System;

namespace Grillbot.Models.TempUnverify.Admin
{
    public class UnverifyAuditFilterRequest
    {
        public ulong? GuildID { get; set; }
        public ulong? FromUserID { get; set; }
        public ulong? DestUserID { get; set; }
        public UnverifyLogOperation? Operation { get; set; }
        public DateTime? DateTimeFrom { get; set; }
        public DateTime? DateTimeTo { get; set; }
    }
}
