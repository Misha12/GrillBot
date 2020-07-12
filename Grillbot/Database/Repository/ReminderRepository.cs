using Grillbot.Database.Entity.Users;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace Grillbot.Database.Repository
{
    public class ReminderRepository : RepositoryBase
    {
        public ReminderRepository(GrillBotContext context) : base(context)
        {
        }

        private IQueryable<Reminder> GetBaseQuery(bool includeUsers)
        {
            var query = Context.Reminders.AsQueryable();

            if (includeUsers)
            {
                query = query
                    .Include(o => o.FromUser)
                    .Include(o => o.User);
            }

            return query;
        }

        public List<Reminder> GetRemindersForInit()
        {
            return GetBaseQuery(false)
                .ToList();
        }

        public Reminder FindReminderByID(long id)
        {
            return GetBaseQuery(true)
                .SingleOrDefault(o => o.RemindID == id);
        }

        public void RemoveRemind(long id)
        {
            var remind = FindReminderByID(id);

            if (remind == null)
                return;

            Context.Reminders.Remove(remind);
            Context.SaveChanges();
        }

        public List<Reminder> GetReminders(long? userId)
        {
            var query = GetBaseQuery(true);

            if (userId != null)
                query.Where(o => o.UserID == userId.Value);

            return query.ToList();
        }

        public bool ExistsReminder(long id)
        {
            return GetBaseQuery(false).Any(o => o.RemindID == id);
        }
    }
}
