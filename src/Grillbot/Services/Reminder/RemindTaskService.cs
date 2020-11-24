using ReminderEntity = Grillbot.Database.Entity.Users.Reminder;
using Grillbot.Models.Reminder;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord.WebSocket;
using Grillbot.Extensions.Discord;
using Discord.Net;
using Grillbot.Enums;
using Discord;
using Grillbot.Services.Initiable;
using Grillbot.Models.Embed;
using Grillbot.Database;

namespace Grillbot.Services.Reminder
{
    public class ReminderTaskService : IInitiable
    {
        private const int RemindersPerTickLimit = 10;

#pragma warning disable IDE0052 // Remove unread private members
        private Timer Timer { get; }
#pragma warning restore

        private ILogger<ReminderTaskService> Logger { get; }
        private IServiceProvider Provider { get; }
        private DiscordSocketClient Discord { get; }
        private BotLoggingService LoggingService { get; }

        public List<ReminderData> Data { get; set; }

        public ReminderTaskService(ILogger<ReminderTaskService> logger, IServiceProvider provider, DiscordSocketClient discord,
            BotLoggingService loggingService)
        {
            Logger = logger;
            Data = new List<ReminderData>();
            Provider = provider;
            Discord = discord;
            LoggingService = loggingService;

            var timeout = TimeSpan.FromMinutes(1);
            Timer = new Timer(ReminderCallback, null, timeout, timeout);
        }

        private void ReminderCallback(object _)
        {
            var remindersToProcess = Data
                .Where(o => (o.At - DateTime.Now).TotalSeconds <= 0);

            var remindersToProcessCount = remindersToProcess.Count();

            if (remindersToProcessCount == 0) return;
            if (remindersToProcessCount > RemindersPerTickLimit)
            {
                Logger.LogWarning($"Fetched reminders: {remindersToProcessCount} to process. Throttled to {RemindersPerTickLimit}");
                remindersToProcess = remindersToProcess.Take(RemindersPerTickLimit);
            }

            var tasks = remindersToProcess
                .Select(o => ProcessReminderAsync(o))
                .ToArray();

            Task.WaitAll(tasks);
            Data.RemoveAll(o => remindersToProcess.Any(x => x.ID == o.ID));
        }

        private async Task ProcessReminderAsync(ReminderData data, bool force = false)
        {
            try
            {
                Logger.LogInformation($"Reminder event triggered: {data.ID} ({data.At})");

                using var scope = Provider.CreateScope();
                using var repository = scope.ServiceProvider.GetService<IGrillBotRepository>();

                var remind = await repository.ReminderRepository.FindReminderByIDAsync(data.ID);

                if (remind == null)
                    return;

                var message = await NotifyUserAsync(remind, force);

                remind.RemindMessageIDSnowflake = message?.Id;
                await repository.CommitAsync();
            }
            catch(Exception ex)
            {
                var message = new LogMessage(LogSeverity.Error, typeof(ReminderTaskService).Name, $"An error occured when executing reminder {data?.ID ?? 0}. (Force: {force})", ex);
                await LoggingService.OnLogAsync(message);
            }
        }

        public async Task ProcessReminderForclyAsync(long id)
        {
            var data = Data.Find(o => o.ID == id);
            await ProcessReminderAsync(data, true);
        }

        public void AddReminder(ReminderEntity reminder)
        {
            Data.Add(new ReminderData()
            {
                At = reminder.At,
                ID = reminder.RemindID
            });
        }

        private async Task<IUserMessage> NotifyUserAsync(ReminderEntity reminder, bool force)
        {
            var guild = Discord.GetGuild(reminder.User.GuildIDSnowflake);

            if (guild == null)
                return null;

            var toUser = await guild.GetUserFromGuildAsync(reminder.User.UserIDSnowflake);
            var fromUser = reminder.FromUser == null ? null : await guild.GetUserFromGuildAsync(reminder.FromUser.UserIDSnowflake);

            try
            {
                var embed = CreateEmbedMessage(fromUser, reminder, force);
                var message = await toUser.SendMessageAsync(embed: embed.Build());

                if (!force)
                {
                    await message.AddReactionsAsync(ReminderDefinitions.AllHourEmojis);
                }

                return message;
            }
            catch (HttpException ex)
            {
                if (ex.DiscordCode.HasValue && ex.DiscordCode.Value == (int)DiscordJsonCodes.CannotSendPM)
                {
                    Logger.LogInformation($"Cannot send private message to user {toUser.GetFullName()} ({toUser.Id}). User have disabled PM.");
                    return null;
                }

                throw;
            }
        }

        private BotEmbed CreateEmbedMessage(SocketGuildUser fromUser, ReminderEntity reminder, bool force)
        {
            var embed = new BotEmbed(Discord.CurrentUser, title: (force ? "Okamžité u" : "U") + "pozornění");

            if (reminder.PostponeCounter > 0)
                embed.AddField("Pozor", $"Toto připomenutí bylo odloženo **{reminder.PostponeCounter}x**.", false);

            embed
                .AddField("ID", $"#{reminder.RemindID}", true);

            if (fromUser != null)
                embed.AddField("Od uživatele", fromUser.GetFullName(), true);

            embed
                .AddField("Zpráva", reminder.Message, false)
                .AddField("Možnosti", "Pokud si přeješ toto upozornění posunout, tak klikni na příslušnou emoji reakci podle počtu hodin.", false);

            return embed;
        }

        public void Init()
        {
            using var scope = Provider.CreateScope();
            var repository = scope.ServiceProvider.GetService<IGrillBotRepository>();

            var query = repository.ReminderRepository.GetRemindersForInit();
            var reminders = query.ToList();
            foreach (var reminder in reminders)
            {
                AddReminder(reminder);
            }

            Logger.LogInformation($"Reminders loaded. Loaded count: {reminders.Count}");
        }

        public Task InitAsync() => Task.FromResult(1);

        public bool TaskExists(long id) => Data.Any(o => o.ID == id);

        public void RemoveTask(long id)
        {
            Data.RemoveAll(o => o.ID == id);
        }
    }
}
