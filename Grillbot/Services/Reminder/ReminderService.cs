using System;
using System.ComponentModel.DataAnnotations;
using Discord;
using Grillbot.Database.Repository;
using Microsoft.Extensions.Logging;
using ReminderEntity = Grillbot.Database.Entity.Users.Reminder;

namespace Grillbot.Services.Reminder
{
    public class ReminderService : IDisposable
    {
        private ReminderRepository ReminderRepository { get; }
        private ReminderTaskService ReminderTaskService { get; }
        private ILogger<ReminderService> Logger { get; }
        private UsersRepository UsersRepository { get; }

        public ReminderService(ReminderRepository reminderRepository, ReminderTaskService reminderTaskService, ILogger<ReminderService> logger,
            UsersRepository usersRepository)
        {
            ReminderRepository = reminderRepository;
            Logger = logger;
            ReminderTaskService = reminderTaskService;
            UsersRepository = usersRepository;
        }

        public void CreateReminder(IGuild guild, IUser fromUser, IUser toUser, DateTime at, string message)
        {
            ValidateReminderCreation(at, message);

            var fromUserEntity = UsersRepository.GetOrCreateUser(guild.Id, fromUser.Id, false, false, false, false, true);
            UsersRepository.SaveChangesIfAny();

            var toUserEntity = UsersRepository.GetOrCreateUser(guild.Id, toUser.Id, false, false, false, false, true);

            var remindEntity = new ReminderEntity()
            {
                At = at,
                FromUserID = fromUser == toUser ? (long?)null : fromUserEntity.ID,
                Message = message
            };

            toUserEntity.Reminders.Add(remindEntity);

            UsersRepository.SaveChanges();
            ReminderTaskService.AddReminder(remindEntity);
        }

        private void ValidateReminderCreation(DateTime at, string message)
        {
            if (DateTime.Now > at)
                throw new ValidationException("Datum a �as notifikace mus� b�t v budoucnosti.");

            if (string.IsNullOrEmpty(message))
                throw new ValidationException("Text mus� b�t uveden.");
        }

        public void Dispose()
        {
            ReminderRepository.Dispose();
            UsersRepository.Dispose();
        }
    }
}