using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Grillbot.Attributes;
using Grillbot.Database.Entity.Users;
using Grillbot.Exceptions;
using Grillbot.Extensions;
using Grillbot.Extensions.Discord;
using Grillbot.Helpers;
using Grillbot.Models.Embed;
using Grillbot.Models.Embed.PaginatedEmbed;
using Grillbot.Services;
using Grillbot.Services.Reminder;

namespace Grillbot.Modules
{
    [Name("Upozornění")]
    [Group("remind")]
    [ModuleID(nameof(ReminderModule))]
    public class ReminderModule : BotModuleBase
    {
        private DiscordSocketClient Discord { get; }

        public ReminderModule(PaginationService pagination, DiscordSocketClient discord, IServiceProvider provider) : base(pagination, provider)
        {
            Discord = discord;
        }

        [Command("get")]
        [Summary("Získej moje upozornění.")]
        public async Task GetRemindsAsync()
        {
            try
            {
                using var service = GetService<ReminderService>();

                var reminders = await service.Service.GetRemindersAsync(Context.Guild, Context.User);

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

        [Command("")]
        [Summary("Upozorni uživatele")]
        [Remarks("Pokud chcete vytvořit upozornění pro sebe, tak použijte parametr `me`, nebo tagněte sebe. Pokud si přejete upozornit někoho jiného, tak jej tagněte. Jiná forma identifikace není podporována.\n" +
            "Pokud uživatel má deaktivované PMs, tak notifikace nebudou mít efekt.\nDatum a čas se zadává v následujících formátech:" +
            "\n- *\"dd/MM/yyyy HH:mm\"*,\n- *\"dd/MM/yyyy HH:mm(:ss)\"*,\n- *ISO 8601*,\n- *\"dd. MM. yyyy HH:mm(:ss)\"*\n**Vteřiny jsou nepovinné.\n" +
            "Nezapomínejte na uvozovky, jinak vám to správně nenačte datum a čas.**")]
        public async Task RemindUserAsync(string user, string at, [Remainder] string message)
        {
            try
            {
                var mentionedUser = (user == "me" ? Context.User : null) ?? Context.Message.MentionedUsers.FirstOrDefault(o => o.Mention == user);

                if(mentionedUser == null)
                {
                    await ReplyAsync($"Hledaný uživatel `{user}` nebyl nalezen.");
                    return;
                }

                var dateTimeAt = StringHelper.ParseDateTime(at);

                using var service = GetService<ReminderService>();
                await service.Service.CreateReminderAsync(Context.Guild, Context.User, mentionedUser, dateTimeAt, message, Context.Message);

                await ReplyAsync($"Upozornění vytvořeno. Pokud si někdo přeje dostat toto upozornění také, tak ať dá na zprávu s příkazem reakci {ReminderDefinitions.CopyRemindEmoji.Name}");
                await Context.Message.AddReactionAsync(ReminderDefinitions.CopyRemindEmoji);
            }
            catch (Exception ex)
            {
                if (ex is ValidationException || ex is FormatException)
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
            using var service = GetService<ReminderService>();
            var reminders = await service.Service.GetAllRemindersAsync();

            if (reminders.Count == 0)
            {
                await ReplyAsync("Nikdo nemá žádné upozornění.");
                return;
            }

            var embed = await CreatePaginatedEmbedAsync(reminders, true);
            await SendPaginatedEmbedAsync(embed);
        }

        [Command("cancel")]
        [Summary("Předčasné ukončení upozornění.")]
        public async Task CancelReminderAsync(long id)
        {
            try
            {
                using var service = GetService<ReminderService>();
                await service.Service.CancelReminderWithoutNotification(id, Context.User as SocketGuildUser);
                await ReplyAsync("Upozornění bylo staženo.");
            }
            catch (Exception ex)
            {
                if (ex is UnauthorizedAccessException || ex is InvalidOperationException)
                {
                    await ReplyAsync(ex.Message);
                    return;
                }

                throw;
            }
        }

        [Command("notify")]
        [Summary("Předčasné upozornění a ukončení upozornění.")]
        [Remarks("Pokud uživatel má deaktivované PMs, tak notifikace nebudou mít efekt.")]
        public async Task NotifyReminderAsync(long id)
        {
            try
            {
                using var service = GetService<ReminderService>();
                await service.Service.CancelReminderWithNotificationAsync(id, Context.User as SocketGuildUser);
                await ReplyAsync("Notifikace a ukončení bylo dokončeno.");
            }
            catch (Exception ex)
            {
                if (ex is UnauthorizedAccessException || ex is InvalidOperationException)
                {
                    await ReplyAsync(ex.Message);
                    return;
                }

                throw;
            }
        }

        [Command("leaderboard")]
        [Summary("Leaderboard uživatelů, kteří nejvíc odkládají připomenutí.")]
        public async Task RemindPostponeLeaderboardAsync()
        {
            using var service = GetService<ReminderService>();
            var leaderboard = await service.Service.GetLeaderboard();

            var builder = new StringBuilder();
            for (int i = 0; i < leaderboard.Count; i++)
            {
                var user = leaderboard[i];
                builder.Append("> ").Append(i + 1).Append(": *").Append(user.Item1.GetDisplayName()).Append("*: **").Append(user.Item2.FormatWithSpaces()).AppendLine("x**");
            }

            var embed = new BotEmbed(Context.User, title: "Leaderboard nejvíce odkládajících osob.")
                .WithDescription(builder.ToString());

            await ReplyAsync(embed: embed.Build());
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

                    page.AddField(title, $"ID: {reminder.RemindID}\nZpráva: {reminder.Message}\nZa: {(reminder.At - DateTime.Now).ToFullCzechTimeString()}");
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
