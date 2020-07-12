using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Grillbot.Attributes;
using Grillbot.Database.Entity.Users;
using Grillbot.Exceptions;
using Grillbot.Extensions;
using Grillbot.Extensions.Discord;
using Grillbot.Models.Embed.PaginatedEmbed;
using Grillbot.Services;
using Grillbot.Services.Permissions.Preconditions;
using Grillbot.Services.Reminder;

namespace Grillbot.Modules
{
    [RequirePermissions]
    [ModuleID("ReminderModule")]
    [Name("Upozornění")]
    [Group("remind")]
    public class ReminderModule : BotModuleBase
    {
        private ReminderService Reminder { get; }
        private DiscordSocketClient Discord { get; }

        public ReminderModule(ReminderService reminder, PaginationService pagination, DiscordSocketClient discord) : base(paginationService: pagination)
        {
            Reminder = reminder;
            Discord = discord;
        }

        [Command("me")]
        [Summary("Upozorni mě.")]
        [Remarks("Pokud uživatel má deaktivované PMs, tak notifikace nebudou mít efekt.")]
        public async Task RemindMeAsync(DateTime at, [Remainder] string message)
        {
            await RemindUserAsync(Context.User, at, message);
        }

        [Command("get")]
        [Summary("Získej moje upozornění.")]
        public async Task GetRemindsAsync()
        {
            try
            {
                var reminders = await Reminder.GetRemindersAsync(Context.Guild, Context.User);

                if (reminders.Count == 0)
                {
                    await ReplyAsync($"{Context.User.Mention} Nemáš žádné upozornění.");
                    return;
                }

                var embed = await CreatePaginatedEmbedAsync(reminders);
                await SendPaginatedEmbedAsync(embed);
            }
            catch (NotFoundException ex)
            {
                await ReplyAsync(ex.Message);
            }
        }

        [Command("user")]
        [Summary("Upozorni uživatele")]
        [Remarks("Pokud uživatel má deaktivované PMs, tak notifikace nebudou mít efekt.")]
        public async Task RemindUserAsync(IUser user, DateTime at, [Remainder] string message)
        {
            try
            {
                Reminder.CreateReminder(Context.Guild, Context.User, user, at, message);
                await ReplyAsync("Upozornění vytvořeno.");
            }
            catch (Exception ex)
            {
                if (ex is ValidationException)
                {
                    await ReplyAsync(ex.Message);
                    return;
                }

                throw;
            }
        }

        [Command("all")]
        [Summary("Získej všechny upozornění.")]
        public async Task GetAllRemindsAsync()
        {
            var reminders = Reminder.GetAllReminders();
            var embed = await CreatePaginatedEmbedAsync(reminders, true);
            await SendPaginatedEmbedAsync(embed);
        }

        [Command("cancel")]
        [Summary("Předčasné ukončení upozornění.")]
        public async Task CancelReminderAsync(long id)
        {
            Reminder.CancelReminderWithoutNotification(id);
            await ReplyAsync("Upozornění bylo staženo.");
        }

        [Command("notify")]
        [Summary("Předčasné upozornění a ukončení upozornění.")]
        [Remarks("Pokud uživatel má deaktivované PMs, tak notifikace nebudou mít efekt.")]
        public async Task NotifyReminderAsync(long id)
        {
            await Reminder.CancelReminderWithNotificationAsync(id);
            await ReplyAsync("Notifikace a ukončení bylo dokončeno.");
        }

        private async Task<PaginatedEmbed> CreatePaginatedEmbedAsync(List<Reminder> reminders, bool full = false)
        {
            var embed = new PaginatedEmbed()
            {
                Pages = new List<PaginatedEmbedPage>(),
                ResponseFor = Context.User,
                Title = full ? "Upozornění" : "Moje upozornění"
            };

            var chunks = reminders.SplitInParts(EmbedBuilder.MaxFieldCount);

            foreach (var chunk in chunks)
            {
                var page = new PaginatedEmbedPage(null);

                foreach (var reminder in chunk)
                {
                    var title = await CreateFieldTitleAsync(reminder, full);
                    if (string.IsNullOrEmpty(title))
                        continue;

                    page.AddField(title, $"ID: {reminder.RemindID}\nZpráva: {reminder.Message}\nZa: {(reminder.At - DateTime.Now).ToCzechLongTimeString()}");
                }

                if (page.AnyField())
                    embed.Pages.Add(page);
            }

            return embed;
        }

        private async Task<string> CreateFieldTitleAsync(Reminder reminder, bool full)
        {
            var guild = Discord.GetGuild(reminder.User.GuildIDSnowflake);

            if (guild == null)
                return null;

            if (reminder.FromUserID == null && !full)
                return "Moje";

            var fromUser = reminder.FromUserID == null ? null : await guild.GetUserFromGuildAsync(reminder.FromUser.UserIDSnowflake);

            if (full)
            {
                var toUser = await guild.GetUserFromGuildAsync(reminder.User.UserIDSnowflake);

                if (reminder.FromUserID == null)
                    return toUser.GetFullName();

                return $"Od uživatele {fromUser.GetFullName()} uživateli {toUser.GetFullName()}";
            }

            return fromUser?.GetFullName();
        }
    }
}
