using System;
using Grillbot.Database.Repository;

namespace Grillbot.Services.Reminder
{
    public class ReminderService : IDisposable
    {
        private ReminderRepository ReminderRepository { get; }

        public ReminderService(ReminderRepository reminderRepository)
        {
            ReminderRepository = reminderRepository;
        }

        // TODO: Init 

        public void Dispose()
        {
            ReminderRepository.Dispose();
        }
    }
}