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

        private IQueryable<AuditLogItem> GetBaseQuery(bool includeUser)
        {
            var query = Context.AuditLogs.AsQueryable();

            if (includeUser)
            {
                query = query
                    .Include(o => o.User)
                    .ThenInclude(o => o.UsedInvite)
                    .ThenInclude(o => o.Creator);
            }

            return query.Include(o => o.Files);
        }

        public IQueryable<AuditLogItem> GetAuditLogsQuery(AuditLogQueryFilter filter)
        {
            var query = GetBaseQuery(true);
            return filter.GetDbQuery(query);
        }

        public IQueryable<AuditLogItem> GetAuditLogsByType(AuditLogType type)
        {
            return GetBaseQuery(false)
                .Where(o => o.Type == type);
        }

        public Task<File> FindFileByFilenameAsync(string filename)
        {
            return Context.Files.AsQueryable()
                .SingleOrDefaultAsync(o => o.AuditLogItemId != null && o.Filename == filename);
        }

        public Task<AuditLogItem> FindItemByIdAsync(long id)
        {
            return GetBaseQuery(false)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public IQueryable<string> GetLastAuditLogIdsQuery(ulong guildId, AuditLogType type)
        {
            var halfYearBack = DateTime.Now.AddMonths(-6);

            return Context.AuditLogs.AsQueryable()
                .Where(o => o.DcAuditLogId != null && o.CreatedAt >= halfYearBack && o.GuildId == guildId.ToString() && o.Type == type)
                .Select(o => o.DcAuditLogId);
        }

        public Task<AuditLogItem> FindLastItemAsync(long userId, AuditLogType type)
        {
            return GetBaseQuery(true)
                .Where(o => o.Type == type && o.UserId == userId)
                .OrderByDescending(o => o.Id)
                .FirstOrDefaultAsync();
        }

        public IQueryable<Tuple<AuditLogType, int>> GetStatsPerTypeQuery()
        {
            return GetBaseQuery(false)
                .GroupBy(o => o.Type)
                .Select(o => Tuple.Create(o.Key, o.Count()));
        }
    }
}
