using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Discord;
using Grillbot.Database.Repository;
using Grillbot.Exceptions;
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
                throw new ValidationException("Datum a èas notifikace musí být v budoucnosti.");

            if (string.IsNullOrEmpty(message))
                throw new ValidationException("Text musí být uveden.");
        }

        public async Task<List<ReminderEntity>> GetRemindersAsync(IGuild guild, IUser user)
        {
            var userId = await UsersRepository.FindUserIDFromDiscordIDAsync(guild.Id, user.Id);

            if (userId == null)
                throw new NotFoundException("Žádná data pro tohoto uživatele nebyly nalezeny.");

            return ReminderRepository.GetReminders(userId);
        }

        public List<ReminderEntity> GetAllReminders()
        {
            return ReminderRepository.GetReminders(null);
        }

        public void Dispose()
        {
            ReminderRepository.Dispose();
            UsersRepository.Dispose();
        }
    }
}