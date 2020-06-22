using Grillbot.Database.Entity.Math;
using Grillbot.Models.Math;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Grillbot.Database.Repository
{
    public class MathRepository : RepositoryBase
    {
        private const int PageSize = 25;

        public MathRepository(GrillBotContext context) : base(context)
        {
        }

        public IQueryable<MathAuditLogItem> GetAuditLog(MathAuditLogFilter filter)
        {
            var query = Context.MathAuditLogs
                .Include(o => o.User)
                .OrderByDescending(o => o.ID)
                .AsQueryable();

            if (filter.Channel != null)
            {
                var channel = filter.Channel.Value.ToString();
                query = query.Where(o => o.ChannelID == channel);
            }

            if (filter.DateTimeFrom != null)
                query = query.Where(o => o.DateTime >= filter.DateTimeFrom.Value);

            if (filter.DateTimeTo != null)
                query = query.Where(o => o.DateTime < filter.DateTimeTo.Value);

            if (filter.GuildID != null)
            {
                var guild = filter.GuildID.Value.ToString();
                query = query.Where(o => o.User.GuildID == guild);
            }

            if (filter.UserID != null)
            {
                var user = filter.UserID.Value.ToString();
                query = query.Where(o => o.User.UserID == user);
            }

            var skip = (filter.Page == 0 ? 0 : filter.Page - 1) * PageSize;
            return query.Skip(skip).Take(PageSize);
        }
    }
}
