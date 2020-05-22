using Discord.Commands;
using Grillbot.Extensions.Discord;
using Grillbot.Models.Embed;
using Grillbot.Services.Preconditions;
using Microsoft.Extensions.Options;
using System;
using System.Globalization;
using System.Threading.Tasks;
using Grillbot.Database.Repository;
using Grillbot.Models.Config.AppSettings;

namespace Grillbot.Modules
{
    [Group("birthday")]
    [Name("Narozeniny")]
    [RequirePermissions]
    public class BirthdaysModule : BotModuleBase
    {
        private BirthdaysRepository Repository { get; }

        public BirthdaysModule(IOptions<Configuration> options, BirthdaysRepository repository) : base(options)
        {
            Repository = repository;
        }

        [Command("")]
        public async Task GetTodayBirthdayAsync()
        {
            var birthdays = await Repository.GetBirthdaysForDayAsync(DateTime.Today, Context.Guild.Id.ToString()).ConfigureAwait(false);

            if (birthdays.Count == 0)
            {
                await ReplyAsync("Dnes nemá nikdo narozeniny.");
                return;
            }

            foreach (var birthday in birthdays)
            {
                var guild = Context.Client.GetGuild(birthday.GuildIDSnowflake);
                var user = await guild.GetUserFromGuildAsync(birthday.ID).ConfigureAwait(false);
                var embed = new BotEmbed(Context.User, null, $"Dnes má narozeniny {user.GetFullName()}", user.GetUserAvatarUrl());

                if (birthday.AcceptAge)
                    embed.AddField(o => o.WithName("Věk").WithValue(birthday.ComputeAge()));

                await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
            }
        }

        [Command("add")]
        [Summary("Přidání svého data narození.")]
        [Remarks("Parametr date je váš datum narození, můžete jej zadat ve formátu dd/MM/yyyy, nebo dd/MM.\nPokud zadáte i rok, tak bude zobrazován i váš věk.")]
        public async Task AddBirthdayAsync(string date)
        {
            var exists = await Repository.ExistsUserAsync(Context.User, Context.Guild.Id.ToString()).ConfigureAwait(false);

            if (exists)
            {
                await ReplyAsync("Tento uživatel už má uložené datum narození.");
                return;
            }

            if (DateTime.TryParseExact(date, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateTime))
            {
                await Repository.AddBirthdayAsync(true, dateTime.Date, Context.Guild.Id, Context.Message.Author.Id).ConfigureAwait(false);
            }
            else if (DateTime.TryParseExact(date, "dd/MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime))
            {
                await Repository.AddBirthdayAsync(false, dateTime.Date, Context.Guild.Id, Context.Message.Author.Id).ConfigureAwait(false);
            }
            else
            {
                await ReplyAsync("Neplatný formát data a času. Povolené jsou pouze `dd/MM/yyyy`, nebo `dd/MM`");
            }

            await ReplyAsync("Datum narození bylo úspěšně přidáno.").ConfigureAwait(false);
            await Context.Message.DeleteAsync(new Discord.RequestOptions() { AuditLogReason = "Add Birthday" }).ConfigureAwait(false);
        }

        [Command("remove")]
        [Summary("Odebrání data narození.")]
        public async Task RemoveAsync()
        {
            var exists = await Repository.ExistsUserAsync(Context.User, Context.Guild.Id.ToString()).ConfigureAwait(false);

            if (!exists)
            {
                await ReplyAsync("Tento uživatle nemá uložené datum narození.");
                return;
            }

            await Repository.RemoveAsync(Context.User, Context.Guild.Id.ToString()).ConfigureAwait(false);
            await ReplyAsync("Datum narození bylo odebráno.").ConfigureAwait(false);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                Repository.Dispose();

            base.Dispose(disposing);
        }
    }
}
