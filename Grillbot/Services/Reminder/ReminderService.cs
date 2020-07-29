using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Grillbot.Database.Repository;
using Grillbot.Enums;
using Grillbot.Exceptions;
using Grillbot.Extensions.Discord;
using Grillbot.Services.MessageCache;
using Microsoft.Extensions.Logging;
using ReminderEntity = Grillbot.Database.Entity.Users.Reminder;

namespace Grillbot.Services.Reminder
{
    public class ReminderService : IDisposable
    {
        private ReminderRepository ReminderRepository { get; }
        private ReminderTaskService ReminderTaskService { get; }
        private UsersRepository UsersRepository { get; }
        private DiscordSocketClient Discord { get; }
        private IMessageCache MessageCache { get; }
        private ILogger<ReminderService> Logger { get; }

        public ReminderService(ReminderRepository reminderRepository, ReminderTaskService reminderTaskService, UsersRepository usersRepository,
            DiscordSocketClient discord, IMessageCache messageCache, ILogger<ReminderService> logger)
        {
            ReminderRepository = reminderRepository;
            ReminderTaskService = reminderTaskService;
            UsersRepository = usersRepository;
            Discord = discord;
            MessageCache = messageCache;
            Logger = logger;
        }

        public void CreateReminder(IGuild guild, IUser fromUser, IUser toUser, DateTime at, string message, IMessage originalMessage)
        {
            ValidateReminderCreation(at, message);

            var fromUserEntity = UsersRepository.GetOrCreateUser(guild.Id, fromUser.Id, false, false, false, false, true, false);
            UsersRepository.SaveChangesIfAny();

            var toUserEntity = UsersRepository.GetOrCreateUser(guild.Id, toUser.Id, false, false, false, false, true, false);

            var remindEntity = new ReminderEntity()
            {
                At = at,
                FromUserID = fromUser == toUser ? (long?)null : fromUserEntity.ID,
                Message = message,
                OriginalMessageIDSnowflake = originalMessage.Id
            };

            toUserEntity.Reminders.Add(remindEntity);

            UsersRepository.SaveChanges();
            ReminderTaskService.AddReminder(remindEntity);
        }

        private void ValidateReminderCreation(DateTime at, string message)
        {
            if (DateTime.Now > at)
                throw new ValidationException("Datum a čas notifikace musí být v budoucnosti.");

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

        public void CancelReminderWithoutNotification(long id, SocketGuildUser user)
        {
            var remind = ReminderRepository.FindReminderByID(id);

            if (remind == null)
                throw new InvalidOperationException("Toto upozornění neexistuje.");

            var hasPerms = user.GuildPermissions.Administrator || user.GuildPermissions.ManageMessages;
            if (remind.User.UserIDSnowflake != user.Id && !hasPerms)
                throw new UnauthorizedAccessException("Na tuto operaci nemáš práva.");

            ReminderTaskService.RemoveTask(id);
        }

        public async Task CancelReminderWithNotificationAsync(long id, SocketGuildUser user)
        {
            var remind = ReminderRepository.FindReminderByID(id);

            if (remind == null)
                throw new InvalidOperationException("Toto upozornění neexistuje.");

            var hasPerms = user.GuildPermissions.Administrator || user.GuildPermissions.ManageMessages;
            if (remind.User.UserIDSnowflake != user.Id && !hasPerms)
                throw new UnauthorizedAccessException("Na tuto operaci nemáš práva.");

            await ReminderTaskService.ProcessReminderForclyAsync(id);
            ReminderTaskService.RemoveTask(id);
        }

        public async Task PostponeReminderAsync(IUserMessage message, SocketReaction reaction)
        {
            if (!(message.Channel is IPrivateChannel))
                return;

            if (!(await CanPostponeRemindAsync(message, reaction)))
            {
                var logMessage = $"Embeds: {message.Embeds.Count}, IsEmoji: {reaction.Emote is Emoji}, Time: {DateTime.UtcNow - message.CreatedAt}";
                Logger.LogInformation($"Skipped postpone remind for {(reaction.User.IsSpecified ? $"UnknownUser({reaction.UserId})" : reaction.User.Value.Username)}\n{logMessage}");

                return;
            }

            var remind = ReminderRepository.FindReminderByMessageId(message.Id);

            if (remind == null)
                return;

            var hours = ReminderDefinitions.EmojiToHourNumberMapping[reaction.Emote as Emoji];

            remind.RemindMessageIDSnowflake = null;
            remind.At = DateTime.Now.AddHours(hours);
            remind.PostponeCounter++;

            await message.DeleteMessageAsync();

            ReminderTaskService.AddReminder(remind);
            ReminderRepository.SaveChanges();
        }

        public async Task<List<Tuple<SocketGuildUser, int>>> GetLeaderboard()
        {
            var stats = ReminderRepository.GetLeaderboard();
            var result = new List<Tuple<SocketGuildUser, int>>();

            foreach (var statItem in stats)
            {
                var guild = Discord.GetGuild(statItem.Item1);

                if (guild == null)
                    continue;

                var user = await guild.GetUserFromGuildAsync(statItem.Item2);

                if (user == null)
                    continue;

                result.Add(Tuple.Create(user, statItem.Item3));
            }

            return result;
        }

        private async Task<bool> CanPostponeRemindAsync(IUserMessage message, SocketReaction reaction)
        {
            try
            {
                if (
                    message.Embeds.Count != 1 ||
                    !(reaction.Emote is Emoji emoji) ||
                    !ReminderDefinitions.AllHourEmojis.Contains(emoji) ||
                    !reaction.User.IsSpecified ||
                    (DateTime.UtcNow - message.CreatedAt).TotalHours >= 12.0d
                )
                    return false;

                var users = await message.GetReactionUsersAsync(emoji, 5).FlattenAsync();

                var containsBot = users.Any(o => o.Id == Discord.CurrentUser.Id);
                var containsUser = users.Any(o => o.Id == reaction.User.Value.Id);

                return containsBot && containsUser;
            }
            catch (HttpException ex) when (ex.DiscordCode != null && ex.DiscordCode == (int)DiscordJsonCodes.UnknownMessage)
            {
                return false;
            }
        }

        public async Task HandleRemindCopyAsync(SocketReaction reaction)
        {
            if (!(reaction.Emote is Emoji emoji)) return;
            if (emoji.Name != ReminderDefinitions.CopyRemindEmoji.Name) return;
            if (!reaction.User.IsSpecified) return;

            var originalRemind = ReminderRepository.FindReminderByOriginalMessage(reaction.MessageId);
            if (originalRemind == null) return;

            var originalGuild = Discord.GetGuild(originalRemind.User.GuildIDSnowflake);
            if (originalGuild == null) return;

            var author = await originalGuild.GetUserFromGuildAsync(originalRemind.User.UserIDSnowflake);
            if (author == null) return;

            if (author.Id == reaction.User.Value.Id)
            {
                await reaction.Channel.SendMessageAsync($"{reaction.User.Value.Mention} Nemůžeš vytvořit kopii ze svého upozornění.");
                return;
            }

            if (originalRemind.At < DateTime.Now) return;
            var origMessageData = reaction.Message.IsSpecified ? reaction.Message.Value : (await MessageCache.GetAsync(reaction.Channel.Id, reaction.MessageId));

            try
            {
                CreateReminder(originalGuild, author, reaction.User.Value, originalRemind.At, originalRemind.Message, origMessageData);
            }
            catch (ValidationException ex)
            {
                await reaction.Channel.SendMessageAsync($"{reaction.User.Value.Mention} {ex.Message}");
            }
        }

        public void Dispose()
        {
            ReminderRepository.Dispose();
            UsersRepository.Dispose();
        }
    }
}
