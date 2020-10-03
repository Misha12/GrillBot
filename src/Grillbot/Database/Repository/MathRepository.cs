using Discord.WebSocket;
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

        public IQueryable<MathAuditLogItem> GetLogData(MathHistoryFilter filter, SocketGuild guild)
        {
            var query = Context.MathAuditLogs
                .Include(o => o.User)
                .Where(o => o.User.GuildID == guild.Id.ToString())
                .OrderByDescending(o => o.ID)
                .AsQueryable();

            if (filter.Channel != null)
            {
                var channelID = filter.Channel.Id.ToString();
                query = query.Where(o => o.ChannelID == channelID);
            }

            if (filter.From != null)
                query = query.Where(o => o.DateTime >= filter.From.Value);

            if (filter.To != null)
                query = query.Where(o => o.DateTime < filter.To.Value);

            if (filter.User != null)
            {
                var userID = filter.User.Id.ToString();
                query = query.Where(o => o.User.UserID == userID);
            }

            var skip = (filter.Page == 0 ? 0 : filter.Page - 1) * PageSize;
            return query.Skip(skip).Take(PageSize);
        }
    }
}
