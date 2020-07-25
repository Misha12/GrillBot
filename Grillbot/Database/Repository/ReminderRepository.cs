using Grillbot.Database.Entity.Users;
using Microsoft.EntityFrameworkCore;
using System;
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
                .Where(o => o.RemindMessageID == null)
                .ToList();
        }

        public Reminder FindReminderByID(long id)
        {
            return GetBaseQuery(true)
                .SingleOrDefault(o => o.RemindID == id);
        }

        public Reminder FindReminderByMessageId(ulong messageId)
        {
            return GetBaseQuery(true)
                .SingleOrDefault(o => o.RemindMessageID == messageId.ToString());
        }

        public Reminder FindReminderByOriginalMessage(ulong messageId)
        {
            return GetBaseQuery(true)
                .SingleOrDefault(o => o.OriginalMessageID == messageId.ToString());
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
                query = query.Where(o => o.UserID == userId.Value);

            return query
                .Where(o => o.At > DateTime.Now)
                .ToList();
        }

        public bool ExistsReminder(long id)
        {
            return GetBaseQuery(false).Any(o => o.RemindID == id);
        }

        public List<Tuple<ulong, ulong, int>> GetLeaderboard()
        {
            return GetBaseQuery(true)
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
    }
}
