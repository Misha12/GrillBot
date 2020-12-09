using Grillbot.Database.Entity;
using Grillbot.Database.Entity.AuditLog;
using Grillbot.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

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

            if (filter.SortDesc)
                query = query.OrderByDescending(o => o.CreatedAt).ThenByDescending(o => o.Id);
            else
                query = query.OrderBy(o => o.CreatedAt).ThenBy(o => o.Id);

            return query.Select(o => new AuditLogItem()
            {
                CreatedAt = o.CreatedAt,
                DcAuditLogId = o.DcAuditLogId,
                Files = o.Files.Select(x => new Entity.File()
                {
                    Filename = x.Filename
                }).ToHashSet(),
                DcAuditLogIdSnowflake = o.DcAuditLogIdSnowflake,
                GuildId = o.GuildId,
                Id = o.Id,
                JsonData = o.JsonData,
                Type = o.Type,
                User = o.User,
                UserId = o.UserId
            });
        }

        public Task<File> FindFileByFilenameAsync(string filename)
        {
            return Context.Files.AsQueryable()
                .SingleOrDefaultAsync(o => o.AuditLogItemId != null && o.Filename == filename);
        }

        public Task<AuditLogItem> FindItemByIdAsync(long id)
        {
            return Context.AuditLogs.AsQueryable()
                .Include(o => o.Files)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public IQueryable<string> GetLastAuditLogIdsQuery(ulong guildId)
        {
            var halfYearBack = DateTime.Now.AddMonths(-6);

            return Context.AuditLogs.AsQueryable()
                .Where(o => o.DcAuditLogId != null && o.CreatedAt >= halfYearBack && o.GuildId == guildId.ToString())
                .Select(o => o.DcAuditLogId);
        }
    }
}
