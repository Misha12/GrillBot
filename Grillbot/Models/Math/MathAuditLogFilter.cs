using System;

namespace Grillbot.Models.Math
{
    public class MathAuditLogFilter
    {
        public ulong? GuildID { get; set; }
        public long? UserID { get; set; }
        public ulong? Channel { get; set; }
        public DateTime? DateTimeFrom { get; set; }
        public DateTime? DateTimeTo { get; set; }
        public int Page { get; set; } = 1;
    }
}
