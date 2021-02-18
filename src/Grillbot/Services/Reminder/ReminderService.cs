using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Grillbot.Database;
using Grillbot.Database.Enums.Includes;
using Grillbot.Enums;
using Grillbot.Exceptions;
using Grillbot.Extensions.Discord;
using Grillbot.Helpers;
using Grillbot.Models.Embed;
using Grillbot.Services.BackgroundTasks;
using Grillbot.Services.Initiable;
using Grillbot.Services.MessageCache;
using Grillbot.Services.UserManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ReminderEntity = Grillbot.Database.Entity.Users.Reminder;

namespace Grillbot.Services.Reminder
{
    public class ReminderService : IBackgroundTaskObserver, IInitiable
    {
        private IGrillBotRepository GrillBotRepository { get; }
        private DiscordSocketClient Discord { get; }
        private IMessageCache MessageCache { get; }
        private ILogger<ReminderService> Logger { get; }
        private SearchService SearchService { get; }
        private BackgroundTaskQueue Queue { get; }
        private UserService UserService { get; }

        public ReminderService(DiscordSocketClient discord, IMessageCache messageCache, ILogger<ReminderService> logger,
            SearchService searchService, IGrillBotRepository grillBotRepository, BackgroundTaskQueue queue, UserService userService)
        {
            Discord = discord;
            MessageCache = messageCache;
            Logger = logger;
            SearchService = searchService;
            GrillBotRepository = grillBotRepository;
            Queue = queue;
            UserService = userService;
        }

        public async Task CreateReminderAsync(IGuild guild, IUser fromUser, IUser toUser, DateTime at, string message, IMessage originalMessage)
        {
            ValidateReminderCreation(at, message);

            var fromUserEntity = await GrillBotRepository.UsersRepository.GetOrCreateUserAsync(guild.Id, fromUser.Id, UsersIncludes.Reminders);
            await GrillBotRepository.CommitAsync();

            var toUserEntity = await GrillBotRepository.UsersRepository.GetOrCreateUserAsync(guild.Id, toUser.Id, UsersIncludes.Reminders);

            var remindEntity = new ReminderEntity()
            {
                At = at,
                FromUserID = fromUser == toUser ? (long?)null : fromUserEntity.ID,
                Message = message,
                OriginalMessageIDSnowflake = originalMessage.Id
            };

            toUserEntity.Reminders.Add(remindEntity);

            await GrillBotRepository.CommitAsync();
            Queue.Add(new ReminderBackgroundTask(remindEntity));
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
            var userId = await SearchService.GetUserIDFromDiscordUserAsync(guild, user);

            if (userId == null)
                throw new NotFoundException("Žádná data pro tohoto uživatele nebyly nalezeny.");

            return await GrillBotRepository.ReminderRepository.GetReminders(userId).ToListAsync();
        }

        public async Task<List<ReminderEntity>> GetAllRemindersAsync()
        {
            return await GrillBotRepository.ReminderRepository.GetReminders(null).ToListAsync();
        }

        public async Task CancelReminderWithoutNotification(long id, SocketGuildUser user)
        {
            var remind = await GrillBotRepository.ReminderRepository.FindReminderByIDAsync(id);

            if (remind == null)
                throw new InvalidOperationException("Toto upozornění neexistuje.");

            await CheckCancellationPermsAsync(remind, user);

            remind.RemindMessageIDSnowflake = ulong.MinValue;
            await GrillBotRepository.CommitAsync();
            Queue.TryRemove<ReminderBackgroundTask>(o => o.Id == id);
        }

        public async Task CancelReminderWithNotificationAsync(long id, SocketGuildUser user)
        {
            var remind = await GrillBotRepository.ReminderRepository.FindReminderByIDAsync(id);

            if (remind == null)
                throw new InvalidOperationException("Toto upozornění neexistuje.");

            await CheckCancellationPermsAsync(remind, user);

            await TriggerReminder(id, true);
            Queue.TryRemove<ReminderBackgroundTask>(o => o.Id == id);
        }

        private async Task CheckCancellationPermsAsync(ReminderEntity entity, SocketGuildUser user)
        {
            if (user.GuildPermissions.Administrator || user.GuildPermissions.ManageMessages)
                return;

            if (user.Id == entity.User.UserIDSnowflake || await UserService.IsBotAdminAsync(user.Guild, user))
                return;

            throw new UnauthorizedAccessException("Pro předčasné zrušení připomenutí nemáš dostatečná práva.");
        }

        public async Task PostponeReminderAsync(IUserMessage message, SocketReaction reaction)
        {
            if (!(message.Channel is IPrivateChannel))
                return;

            if (!await CanPostponeRemindAsync(message, reaction))
            {
                var logMessage = $"Embeds: {message.Embeds.Count}, IsEmoji: {reaction.Emote is Emoji}, Time: {DateTime.UtcNow - message.CreatedAt}";
                Logger.LogInformation($"Skipped postpone remind for {(reaction.User.IsSpecified ? $"UnknownUser({reaction.UserId})" : reaction.User.Value.Username)}\n{logMessage}");

                return;
            }

            var remind = await GrillBotRepository.ReminderRepository.FindReminderByMessageIdAsync(message.Id);

            if (remind == null)
                return;

            var hours = EmojiHelper.EmojiToIntMap[reaction.Emote as Emoji];

            remind.RemindMessageIDSnowflake = null;
            remind.At = DateTime.Now.AddHours(hours);
            remind.PostponeCounter++;

            await message.DeleteMessageAsync();
            await GrillBotRepository.CommitAsync();
            Queue.Add(new ReminderBackgroundTask(remind));
        }

        public async Task<List<Tuple<SocketGuildUser, int>>> GetLeaderboard()
        {
            var stats = GrillBotRepository.ReminderRepository.GetLeaderboard();
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
                    reaction.Emote is not Emoji emoji ||
                    EmojiHelper.EmojiToIntMap.ContainsKey(emoji) ||
                    !reaction.User.IsSpecified ||
                    (DateTime.UtcNow - message.CreatedAt).TotalHours >= 24.0d
                )
                {
                    return false;
                }

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
            if (reaction.Emote is not Emoji emoji || emoji.Name != EmojiHelper.PersonRisingHand.Name || !reaction.User.IsSpecified) return;

            var originalRemind = await GrillBotRepository.ReminderRepository.FindReminderByOriginalMessageAsync(reaction.MessageId);
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
                await CreateReminderAsync(originalGuild, author, reaction.User.Value, originalRemind.At, originalRemind.Message, origMessageData);
            }
            catch (ValidationException ex)
            {
                await reaction.Channel.SendMessageAsync($"{reaction.User.Value.Mention} {ex.Message}");
            }
        }

        public async Task TriggerBackgroundTaskAsync(object data)
        {
            if (data is not ReminderBackgroundTask task)
                return;

            await TriggerReminder(task.Id, false);
        }

        public async Task TriggerReminder(long id, bool force)
        {
            var entity = await GrillBotRepository.ReminderRepository.FindReminderByIDAsync(id);

            if (entity == null)
                return;

            var message = await NotifyUserAsync(entity, force);
            entity.RemindMessageIDSnowflake = message?.Id;
            await GrillBotRepository.CommitAsync();
        }

        public void Init() { }

        public async Task InitAsync()
        {
            var reminders = await GrillBotRepository.ReminderRepository.GetRemindersForInit().ToListAsync();

            foreach (var reminder in reminders)
            {
                var task = new ReminderBackgroundTask(reminder);
                Queue.Add(task);
            }

            Logger.LogInformation($"Reminders loaded. Loaded count: {reminders.Count}");
        }

        private async Task<IUserMessage> NotifyUserAsync(ReminderEntity entity, bool force)
        {
            var guild = Discord.GetGuild(entity.User.GuildIDSnowflake);

            if (guild == null)
                return null;

            var toUser = await guild.GetUserFromGuildAsync(entity.User.UserIDSnowflake);

            if (toUser == null)
                return null;

            var fromUser = entity.FromUser == null ? null : await guild.GetUserFromGuildAsync(entity.FromUser.UserIDSnowflake);

            try
            {
                var embed = CreateRemindEmbed(fromUser, entity, force);
                var message = await toUser.SendMessageAsync(embed: embed.Build());
                await message.AddReactionsAsync(EmojiHelper.EmojiToIntMap.Keys.ToArray());

                return message;
            }
            catch (HttpException ex)
            {
                if (ex.DiscordCode == (int)DiscordJsonCodes.CannotSendPM)
                {
                    Logger.LogInformation($"Cannot send private message to user {toUser.GetFullName()} ({toUser.Id}). User have disabled PM.");
                    return null;
                }

                throw;
            }
        }

        private BotEmbed CreateRemindEmbed(SocketGuildUser fromUser, ReminderEntity entity, bool force)
        {
            var embed = new BotEmbed(Discord.CurrentUser, title: (force ? "Okamžité u" : "U") + "pozornění");

            if(entity.PostponeCounter > 0)
                embed.AddField("Pozor", $"Toto připomenutí bylo odloženo **{entity.PostponeCounter}x**.", false);

            embed
                .AddField("ID", $"#{entity.RemindID}", true);

            if (fromUser != null)
                embed.AddField("Od uživatele", fromUser.GetFullName(), true);

            embed
                .AddField("Zpráva", entity.Message, false)
                .AddField("Možnosti", "Pokud si přeješ toto upozornění posunout, tak klikni na příslušnou emoji reakci podle počtu hodin.", false);

            return embed;
        }
    }
}
