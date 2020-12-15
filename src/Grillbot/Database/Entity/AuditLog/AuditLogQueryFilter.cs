using Grillbot.Enums;
using System;
using System.Collections.Generic;

namespace Grillbot.Database.Entity.AuditLog
{
    public class AuditLogQueryFilter
    {
        public string GuildId { get; set; }
        public List<long> UserIds { get; set; }
        public bool SortDesc { get; set; }
        public AuditLogType? Type { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public List<long> IgnoredIds { get; set; }
    }
}
