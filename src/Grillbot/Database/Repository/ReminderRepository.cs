using Grillbot.Database.Entity.Users;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Database.Repository
{
    public class ReminderRepository : RepositoryBase
    {
        public ReminderRepository(GrillBotContext context) : base(context)
        {
        }

        private IQueryable<Reminder> GetBaseQuery(bool includeFrom, bool includeTo)
        {
            var query = Context.Reminders.AsQueryable();

            if (includeFrom)
                query = query.Include(o => o.FromUser);

            if (includeTo)
                query = query.Include(o => o.User);

            return query;
        }

        public IQueryable<Reminder> GetRemindersForInit()
        {
            return GetBaseQuery(false, false)
                .Where(o => o.RemindMessageID == null);
        }

        public Task<Reminder> FindReminderByIDAsync(long id)
        {
            return GetBaseQuery(true, true)
                .SingleOrDefaultAsync(o => o.RemindID == id);
        }

        public Task<Reminder> FindReminderByMessageIdAsync(ulong messageId)
        {
            return GetBaseQuery(true, true)
                .SingleOrDefaultAsync(o => o.RemindMessageID == messageId.ToString());
        }

        public Task<Reminder> FindReminderByOriginalMessageAsync(ulong messageId)
        {
            return GetBaseQuery(true, true)
                .FirstOrDefaultAsync(o => o.OriginalMessageID == messageId.ToString());
        }

        public IQueryable<Reminder> GetReminders(long? userId)
        {
            var query = GetBaseQuery(true, true);

            if (userId != null)
                query = query.Where(o => o.UserID == userId.Value);

            return query
                .Where(o => o.At > DateTime.Now && o.RemindMessageID != null)
                .OrderBy(o => o.At);
        }

        public List<Tuple<ulong, ulong, int>> GetLeaderboard()
        {
            return GetBaseQuery(true, true)
                .Where(o => o.PostponeCounter > 0)
                .AsEnumerable()
                .GroupBy(o => o.UserID)
                .Select(o =>
                {
                    var item = o.First().User;
                    return Tuple.Create(item.GuildIDSnowflake, item.UserIDSnowflake, o.Sum(o => o.PostponeCounter));
                })
                .Take(10)
                .OrderByDescending(o => o.Item3)
                .ToList();
        }

        public IQueryable<Reminder> GetRemindersOfUser(long id)
        {
            return GetBaseQuery(true, false)
                .Where(o => o.UserID == id);
        }
    }
}
