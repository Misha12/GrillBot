using ReminderEntity = Grillbot.Database.Entity.Users.Reminder;
using Grillbot.Models.Reminder;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Grillbot.Database.Repository;
using Discord.WebSocket;
using Grillbot.Extensions.Discord;
using System.Text;
using Discord.Net;
using Grillbot.Enums;
using Discord;
using Grillbot.Services.Initiable;

namespace Grillbot.Services.Reminder
{
    public class ReminderTaskService : IInitiable
    {
#pragma warning disable IDE0052 // Remove unread private members
        private Timer Timer { get; }
#pragma warning restore

        private ILogger<ReminderTaskService> Logger { get; }
        private IServiceProvider Provider { get; }
        private DiscordSocketClient Discord { get; }

        public List<ReminderData> Data { get; set; }

        public ReminderTaskService(ILogger<ReminderTaskService> logger, IServiceProvider provider, DiscordSocketClient discord)
        {
            Logger = logger;
            Data = new List<ReminderData>();
            Provider = provider;
            Discord = discord;

            var timeout = TimeSpan.FromMinutes(1);
            Timer = new Timer(ReminderCallback, null, timeout, timeout);
        }

        private void ReminderCallback(object _)
        {
            var remindersToProcess = Data
                .Where(o => (o.At - DateTime.Now).TotalSeconds <= 0);

            var tasks = remindersToProcess
                .Select(o => ProcessReminderAsync(o))
                .ToArray();

            Task.WaitAll(tasks);
            Data.RemoveAll(o => remindersToProcess.Any(x => x.ID == o.ID));
        }

        private async Task ProcessReminderAsync(ReminderData data)
        {
            Logger.LogInformation($"Reminder event triggered: {data.ID} ({data.At})");

            using var scope = Provider.CreateScope();
            using var remindersRepository = scope.ServiceProvider.GetService<ReminderRepository>();

            var remind = remindersRepository.FindReminderByID(data.ID);

            if (remind == null)
                return;

            await NotifyUserAsync(remind);
            remindersRepository.RemoveRemind(data.ID);
        }

        public void AddReminder(ReminderEntity reminder)
        {
            Data.Add(new ReminderData()
            {
                At = reminder.At,
                ID = reminder.RemindID
            });
        }

        private async Task NotifyUserAsync(ReminderEntity reminder)
        {
            var guild = Discord.GetGuild(reminder.User.GuildIDSnowflake);

            if (guild == null)
                return;

            var toUser = await guild.GetUserFromGuildAsync(reminder.User.UserIDSnowflake);
            var fromUser = reminder.FromUser == null ? null : await guild.GetUserFromGuildAsync(reminder.FromUser.UserIDSnowflake);

            try
            {
                var message = CreateMessage(fromUser, reminder.Message);
                await toUser.SendMessageAsync(message);
            }
            catch (HttpException ex)
            {
                if (ex.DiscordCode.HasValue && ex.DiscordCode.Value == (int)DiscordJsonCodes.CannotSendPM)
                {
                    Logger.LogInformation($"Cannot send private message to user {toUser.GetFullName()} ({toUser.Id}). User have disabled PM.");
                    return;
                }

                throw;
            }
        }

        private string CreateMessage(SocketGuildUser fromUser, string message)
        {
            var builder = new StringBuilder()
                .AppendLine("Ahoj");

            if (fromUser == null)
                builder.AppendLine("Upozoròuji tì, že sis nastavil upozornìní.");
            else
                builder.AppendLine($"Upozoròuji tì, že máš od uživatele {fromUser.GetFullName()} upozornìní.");

            return builder
                .Append("Máš k tomu i zprávu: ")
                .AppendLine(message)
                .ToString();
        }

        public void Init()
        {
            using var scope = Provider.CreateScope();
            using var remindersRepository = scope.ServiceProvider.GetService<ReminderRepository>();

            var reminders = remindersRepository.GetRemindersForInit();
            foreach (var reminder in reminders)
            {
                AddReminder(reminder);
            }

            Logger.LogInformation($"Reminders loaded. Loaded count: {reminders.Count}");
        }

        public Task InitAsync() => Task.FromResult(1);
    }
}