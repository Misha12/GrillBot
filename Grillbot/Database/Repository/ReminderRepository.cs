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

            if(includeUsers)
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

        // TODO: Remove remind from database
        // TODO: Get all reminders
        // TODO: Get reminders for specific user.
    }
}