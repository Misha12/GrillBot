using ReminderEntity = Grillbot.Database.Entity.Users.Reminder;
using Grillbot.Models.Reminder;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grillbot.Services.Reminder
{
    public class ReminderTaskService
    {
#pragma warning disable IDE0052 // Remove unread private members
        private Timer Timer { get; }
#pragma warning restore

        private ILogger<ReminderTaskService> Logger { get; }

        public List<ReminderData> Data { get; set; }

        public ReminderTaskService(ILogger<ReminderTaskService> logger)
        {
            Logger = logger;
            Data = new List<ReminderData>();

            var timeout = TimeSpan.FromMinutes(1);
            Timer = new Timer(ReminderCallback, null, timeout, timeout);
        }

        private void ReminderCallback(object _)
        {
            var remindersToProcess = Data
                .Where(o => (DateTime.Now - o.At).TotalSeconds <= 0)
                .Select(o => ProcessReminder(o))
                .ToArray();

            Task.WaitAll(remindersToProcess);
        }

        private Task ProcessReminder(ReminderData data)
        {
            Logger.LogInformation($"Reminder event triggered: {data.ID} ({data.At})");

            // TODO: Process reminder.

            return Task.FromResult(true);
        }

        public void AddReminder(ReminderEntity reminder)
        {
            Data.Add(new ReminderData()
            {
                At = reminder.At,
                ID = reminder.RemindID
            });
        }
    }
}