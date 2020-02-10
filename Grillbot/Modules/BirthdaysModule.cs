using Discord.Commands;
using Grillbot.Extensions.Discord;
using Grillbot.Models.Embed;
using Grillbot.Database;
using Grillbot.Services.Config.Models;
using Grillbot.Services.Preconditions;
using Microsoft.Extensions.Options;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Grillbot.Modules
{
    [Group("birthday")]
    [Name("Narozeniny")]
    [RequirePermissions]
    public class BirthdaysModule : BotModuleBase
    {
        public BirthdaysModule(IOptions<Configuration> options) : base(options)
        {
        }

        [Command("")]
        public async Task GetTodayBirthdayAsync()
        {
            await DoAsync(async () =>
            {
                using(var repository = new GrillBotRepository(Config))
                {
                    var birthdays = await repository.Birthdays.GetBirthdaysForDayAsync(DateTime.Today, Context.Guild.Id.ToString()).ConfigureAwait(false);

                    if (birthdays.Count == 0)
                        throw new ArgumentException("Dnes nemá nikdo narozeniny.");

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
            }).ConfigureAwait(false);
        }

        [Command("add")]
        [Summary("Přidání svého data narození.")]
        [Remarks("Parametr date je váš datum narození, můžete jej zadat ve formátu dd/MM/yyyy, nebo dd/MM.\nPokud zadáte i rok, tak bude zobrazován i váš věk.")]
        public async Task AddBirthdayAsync(string date)
        {
            await DoAsync(async () =>
            {
                using(var repository = new GrillBotRepository(Config))
                {
                    var exists = await repository.Birthdays.ExistsUserAsync(Context.User, Context.Guild.Id.ToString()).ConfigureAwait(false);

                    if (exists)
                        throw new ArgumentException("Tento uživatel už má uložené datum narození.");

                    if (DateTime.TryParseExact(date, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateTime))
                    {
                        await repository.Birthdays.AddBirthdayAsync(true, dateTime.Date, Context).ConfigureAwait(false);
                    }
                    else if (DateTime.TryParseExact(date, "dd/MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime))
                    {
                        await repository.Birthdays.AddBirthdayAsync(false, dateTime.Date, Context).ConfigureAwait(false);
                    }
                    else
                    {
                        throw new ArgumentException("Neplatný formát data a času. Povolené jsou pouze `dd/MM/yyyy`, nebo `dd/MM`");
                    }

                    await ReplyAsync("Datum narození bylo úspěšně přidáno.").ConfigureAwait(false);
                    await Context.Message.DeleteAsync(new Discord.RequestOptions() { AuditLogReason = "Add Birthday" }).ConfigureAwait(false);
                }
            }).ConfigureAwait(false);
        }

        [Command("remove")]
        [Summary("Odebrání data narození.")]
        public async Task RemoveAsync()
        {
            await DoAsync(async () =>
            {
                using(var repository = new GrillBotRepository(Config))
                {
                    var exists = await repository.Birthdays.ExistsUserAsync(Context.User, Context.Guild.Id.ToString()).ConfigureAwait(false);

                    if (!exists)
                        throw new ArgumentException("Tento uživatle nemá uložené datum narození.");

                    await repository.Birthdays.RemoveAsync(Context.User, Context.Guild.Id.ToString()).ConfigureAwait(false);
                    await ReplyAsync("Datum narození bylo odebráno.").ConfigureAwait(false);
                }
            }).ConfigureAwait(false);
        }
    }
}
