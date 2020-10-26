using Grillbot.Database.Entity.Unverify;
using Grillbot.Database.Enums;
using Grillbot.Models.Unverify;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Database.Repository
{
    public class UnverifyRepository : RepositoryBase
    {
        public UnverifyRepository(GrillBotContext context) : base(context)
        {
        }

        public UnverifyLog SaveLogOperation(UnverifyLogOperation operation, JObject jsonData, long fromUserID, long toUserID)
        {
            var entity = new UnverifyLog()
            {
                CreatedAt = DateTime.Now,
                FromUserID = fromUserID,
                Json = jsonData,
                Operation = operation,
                ToUserID = toUserID
            };

            Context.UnverifyLogs.Add(entity);
            Context.SaveChanges();

            return entity;
        }

        public async Task RemoveUnverifyAsync(ulong guildID, ulong userID)
        {
            var unverify = await Context.Unverifies.AsQueryable()
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.User.GuildID == guildID.ToString() && o.User.UserID == userID.ToString());

            if (unverify == null)
                return;

            Context.Unverifies.Remove(unverify);
            await Context.SaveChangesAsync();
        }

        public Unverify FindUnverifyByID(long id)
        {
            return Context.Unverifies.Include(o => o.User)
                .FirstOrDefault(o => o.UserID == id);
        }

        public IQueryable<UnverifyLog> GetLogs(UnverifyAuditFilter filter, bool disablePagination = false)
        {
            var query = Context.UnverifyLogs.AsQueryable()
                .Include(o => o.FromUser)
                .Include(o => o.ToUser)
                .Where(o => o.FromUser.GuildID == filter.Guild.Id.ToString() && o.ToUser.GuildID == filter.Guild.Id.ToString());

            if (filter.FromUsers.Count > 0)
            {
                var fromUsersIds = filter.FromUsers.Select(o => o.Id.ToString()).ToArray();
                query = query.Where(o => fromUsersIds.Contains(o.FromUser.UserID));
            }

            if (filter.ToUsers.Count > 0)
            {
                var toUsersQuery = filter.ToUsers.Select(o => o.Id.ToString()).ToArray();
                query = query.Where(o => toUsersQuery.Contains(o.ToUser.UserID));
            }

            if (filter.Operation != null)
                query = query.Where(o => o.Operation == filter.Operation.Value);

            if (filter.DateTimeFrom != null)
                query = query.Where(o => o.CreatedAt >= filter.DateTimeFrom.Value);

            if (filter.DateTimeTo != null)
                query = query.Where(o => o.CreatedAt < filter.DateTimeTo.Value);

            if (filter.OrderAsc)
                query = query.OrderBy(o => o.ID);
            else
                query = query.OrderByDescending(o => o.ID);

            if (disablePagination)
                return query;

            return query
                .Skip(filter.Skip)
                .Take(filter.Take);
        }

        public Task<bool> HaveUnverifyAsync(long userID)
        {
            return Context.Unverifies.AsQueryable()
                .AnyAsync(o => o.UserID == userID);
        }
    }
}
