using Grillbot.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grillbot.Models.Audit
{
    public class LogsFilter
    {
        public ulong GuildId { get; set; }
        public string UserQuery { get; set; }
        public bool SortDesc { get; set; } = true;
        public string Types { get; set; }
        public int Page { get; set; } = 1;
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public bool IgnoreBots { get; set; }

        public IEnumerable<AuditLogType> GetSelectedTypes()
        {
            if (string.IsNullOrEmpty(Types))
                return Enumerable.Empty<AuditLogType>();

            return Types.Split('|')
                .Select(o => Enum.TryParse(o.Trim(), out AuditLogType type) ? type : (AuditLogType?)null)
                .Where(o => o != null)
                .Select(o => o.Value);
        }
    }
}
