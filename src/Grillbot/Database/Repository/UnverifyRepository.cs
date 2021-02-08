using Grillbot.Database.Entity.Unverify;
using Grillbot.Models.Unverify;
using Microsoft.EntityFrameworkCore;
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

        public Task<Unverify> FindUnverifyByUser(ulong guildId, ulong userId)
        {
            return Context.Unverifies.AsQueryable()
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.User.GuildID == guildId.ToString() && o.User.UserID == userId.ToString());
        }

        public Task<Unverify> FindUnverifyByID(long id)
        {
            return Context.Unverifies
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.UserID == id);
        }

        public IQueryable<UnverifyLog> GetLogs(UnverifyAuditFilter filter, bool disablePagination = false)
        {
            var query = Context.UnverifyLogs.AsQueryable()
                .Include(o => o.FromUser)
                .Include(o => o.ToUser)
                .Where(o => o.FromUser.GuildID == filter.Guild.Id.ToString() && o.ToUser.GuildID == filter.Guild.Id.ToString());

            if (filter.FromUsers != null)
            {
                var fromUsersIds = filter.FromUsers.Select(o => o.Id.ToString()).ToArray();
                query = query.Where(o => fromUsersIds.Contains(o.FromUser.UserID));
            }

            if (filter.ToUsers != null)
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

        public Task<UnverifyLog> FindLogItemByIDAsync(long id)
        {
            return Context.UnverifyLogs.AsQueryable()
                .Include(o => o.ToUser)
                .ThenInclude(o => o.Unverify)
                .FirstOrDefaultAsync(o => o.ID == id);
        }

        public IQueryable<UnverifyLog> GetIncomingUnverifies(long userId)
        {
            return Context.UnverifyLogs.AsQueryable()
                .Include(o => o.FromUser)
                .Include(o => o.ToUser)
                .Where(o => o.ToUserID == userId);
        }

        public IQueryable<UnverifyLog> GetOutgoingUnverifies(long userId)
        {
            return Context.UnverifyLogs.AsQueryable()
                .Include(o => o.FromUser)
                .Include(o => o.ToUser)
                .Where(o => o.FromUserID == userId);
        }
    }
}
