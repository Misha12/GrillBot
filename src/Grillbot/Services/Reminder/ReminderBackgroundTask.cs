using Grillbot.Models;
using System;

namespace Grillbot.Services.Reminder
{
    public class ReminderBackgroundTask : BackgroundTask<ReminderService>
    {
        public long Id { get; set; }
        public DateTime At { get; set; }

        public override bool CanProcess()
        {
            return DateTime.Now > At;
        }

        public ReminderBackgroundTask(Grillbot.Database.Entity.Users.Reminder entity)
        {
            Id = entity.RemindID;
            At = entity.At;
        }
    }
}
