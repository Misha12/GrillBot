using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Grillbot.Attributes;
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

        public ReminderModule(ReminderService reminder)
        {
            Reminder = reminder;
        }

        [Command("me")]
        [Summary("Upozorni mě.")]
        public async Task RemindMeAsync(DateTime at, [Remainder] string message)
        {
            await RemindUserAsync(Context.User, at, message);
        }

        [Command("get")]
        [Summary("Získej moje upozornění.")]
        public async Task GetRemindsAsync()
        {

        }

        [Command("")]
        [Summary("Upozorni uživatele")]
        public async Task RemindUserAsync(IUser user, DateTime at, [Remainder] string message)
        {
            try
            {
                Reminder.CreateReminder(Context.Guild, Context.User, user, at, message);
                await ReplyAsync("Upozornění vytvořeno.");
            }
            catch(Exception ex)
            {
                if(ex is ValidationException)
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

        }

        [Command("cancel")]
        [Summary("Předčasné ukončení upozornění.")]
        [Remarks("Pokud se poslední parametr nastaví na true, tak dojde k notifikaci uživatelů.")]
        public async Task CancelReminderAsync(int id, bool notify = false)
        {

        }
    }
}