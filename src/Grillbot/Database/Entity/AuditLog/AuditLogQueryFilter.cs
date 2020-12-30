using Grillbot.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grillbot.Database.Entity.AuditLog
{
    public class AuditLogQueryFilter
    {
        public string GuildId { get; set; }
        public List<long> UserIds { get; set; }
        public bool SortDesc { get; set; }
        public AuditLogType[] Types { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public List<long> IgnoredIds { get; set; }

        public IQueryable<AuditLogItem> GetDbQuery(IQueryable<AuditLogItem> query)
        {
            query = query.Where(o => o.GuildId == GuildId);

            if (UserIds != null)
                query = query.Where(o => UserIds.Contains(o.UserId.Value));

            if (Types?.Length > 0)
                query = query.Where(o => Types.Contains(o.Type));

            if (From != null)
                query = query.Where(o => o.CreatedAt >= From.Value);

            if (To != null)
                query = query.Where(o => o.CreatedAt < To.Value);

            if (IgnoredIds?.Count > 0)
                query = query.Where(o => !IgnoredIds.Contains(o.UserId.Value));

            if (SortDesc)
                return query.OrderByDescending(o => o.CreatedAt).ThenByDescending(o => o.Id);
            else
                return query.OrderBy(o => o.CreatedAt).ThenBy(o => o.Id);
        }
    }
}
