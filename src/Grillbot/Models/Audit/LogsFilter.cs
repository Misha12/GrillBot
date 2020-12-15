using Grillbot.Enums;
using System;

namespace Grillbot.Models.Audit
{
    public class LogsFilter
    {
        public ulong GuildId { get; set; }
        public string UserQuery { get; set; }
        public bool SortDesc { get; set; } = true;
        public AuditLogType? Type { get; set; }
        public int Page { get; set; } = 1;
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public bool IgnoreBots { get; set; }
    }
}
