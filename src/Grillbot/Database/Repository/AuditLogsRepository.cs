using Grillbot.Database.Entity.AuditLog;
using Grillbot.Enums;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Grillbot.Database.Repository
{
    public class AuditLogsRepository : RepositoryBase
    {
        public AuditLogsRepository(GrillBotContext context) : base(context)
        {
        }

        public IQueryable<AuditLogItem> GetAuditLogsQuery(AuditLogQueryFilter filter)
        {
            var query = Context.AuditLogs.AsQueryable()
                .Include(o => o.User)
                .ThenInclude(o => o.UsedInvite)
                .ThenInclude(o => o.Creator)
                .Where(o => o.GuildId == filter.GuildId);

            if (filter.UserIds?.Count > 0)
            {
                if (!filter.IncludeAnonymous)
                    query = query.Where(o => o.UserId != null && filter.UserIds.Contains(o.UserId.Value));
                else
                    query = query.Where(o => o.UserId == null || filter.UserIds.Contains(o.UserId.Value));
            }
            else
            {
                if (!filter.IncludeAnonymous)
                    query = query.Where(o => o.UserId != null);
            }

            if (filter.Type != null)
                query = query.Where(o => o.Type == filter.Type.Value);

            if (filter.From != null)
                query = query.Where(o => o.CreatedAt >= filter.From.Value);

            if (filter.To != null)
                query = query.Where(o => o.CreatedAt < filter.To.Value);

            switch(filter.Order)
            {
                case AuditLogOrder.Server when filter.SortDesc:
                    return query.OrderByDescending(o => o.GuildId).ThenByDescending(o => o.Id);
                case AuditLogOrder.Server when !filter.SortDesc:
                    return query.OrderBy(o => o.GuildId).ThenBy(o => o.Id);
                case AuditLogOrder.User when filter.SortDesc:
                    return query.OrderByDescending(o => o.UserId).ThenByDescending(o => o.Id);
                case AuditLogOrder.User when !filter.SortDesc:
                    return query.OrderBy(o => o.UserId).ThenBy(o => o.Id);
                default:
                    if (filter.SortDesc)
                        return query.OrderByDescending(o => o.CreatedAt).ThenByDescending(o => o.Id);
                    else
                        return query.OrderBy(o => o.CreatedAt).ThenBy(o => o.Id);
            }
        }
    }
}
