using Discord.Commands;
using Grillbot.Extensions.Discord;
using Microsoft.Extensions.Options;
using System;
using System.Globalization;
using System.Threading.Tasks;
using Grillbot.Models.Config.AppSettings;
using Grillbot.Services.UserManagement;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Discord;
using Grillbot.Extensions;
using Grillbot.Services;
using Grillbot.Attributes;
using Grillbot.Models.Embed.PaginatedEmbed;

namespace Grillbot.Modules
{
    [Group("birthday")]
    [Name("Narozeniny")]
    [ModuleID("BirthdaysModule")]
    public class BirthdaysModule : BotModuleBase
    {
        private BirthdayService BirthdayService { get; }

        public BirthdaysModule(IOptions<Configuration> options, BirthdayService birthdayService, PaginationService paginationService)
            : base(options, paginationService: paginationService)
        {
            BirthdayService = birthdayService;
        }

        [Command("")]
        public async Task GetTodayBirthdayAsync()
        {
            var birthdayUsers = await BirthdayService.GetUsersWithTodayBirthdayAsync(Context.Guild);

            if (birthdayUsers.Count == 0)
            {
                await ReplyAsync("Dnes nemá nikdo narozeniny.");
                return;
            }

            var pages = new List<PaginatedEmbedPage>();

            foreach (var user in birthdayUsers)
            {
                var page = new PaginatedEmbedPage($"**{user.User.GetFullName()}**", thumbnail: user.User.GetUserAvatarUrl());

                if (user.Birthday.AcceptAge)
                    page.AddField(new EmbedFieldBuilder().WithName("Věk").WithValue(user.Birthday.DateTime.ComputeDateAge()));

                pages.Add(page);
            }

            var embed = new PaginatedEmbed()
            {
                Pages = pages,
                ResponseFor = Context.User,
                Title = "Dnes má narozeniny"
            };

            await SendPaginatedEmbedAsync(embed);
        }

        [Command("add")]
        [Summary("Přidání svého data narození.")]
        [Remarks("Parametr date je váš datum narození, můžete jej zadat ve formátu dd/MM/yyyy, nebo dd/MM.\nPokud zadáte i rok, tak bude zobrazován i váš věk.")]
        public async Task AddBirthdayAsync(string date)
        {
            try
            {
                if (DateTime.TryParseExact(date, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateTime))
                {
                    BirthdayService.SetBirthday(Context.Guild, Context.User, dateTime, true);
                }
                else if (DateTime.TryParseExact(date, "dd/MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime))
                {
                    BirthdayService.SetBirthday(Context.Guild, Context.User, dateTime, false);
                }
                else
                {
                    throw new ValidationException("Neplatný formát data a času. Povolené jsou pouze `dd/MM/yyyy`, nebo `dd/MM`");
                }

                await ReplyAsync("Datum narození bylo úspěšně přidáno.");
            }
            catch (ValidationException ex)
            {
                await ReplyAsync(ex.Message);
            }

            await Context.Message.DeleteAsync(new Discord.RequestOptions() { AuditLogReason = $"Birthday create for {Context.User.GetShortName()}" });
        }

        [Command("remove")]
        [Summary("Odebrání data narození.")]
        public async Task RemoveAsync()
        {
            try
            {
                BirthdayService.ClearBirthday(Context.Guild, Context.User);
                await ReplyAsync("Datum narození bylo odebráno.");
            }
            catch (ValidationException ex)
            {
                await ReplyAsync(ex.Message);
            }
        }

        [Command("have?")]
        [Summary("Mám uložené narozeniny?")]
        public async Task Have()
        {
            var have = await BirthdayService.HaveUserBirthday(Context.Guild, Context.User);
            await ReplyAsync($"{Context.User.Mention} {(have ? "máš" : "nemáš")} uložené narozeniny.");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                BirthdayService.Dispose();

            base.Dispose(disposing);
        }
    }
}
