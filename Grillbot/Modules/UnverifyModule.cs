using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Grillbot.Attributes;
using Grillbot.Exceptions;
using Grillbot.Extensions;
using Grillbot.Extensions.Discord;
using Grillbot.Models.Embed;
using Grillbot.Models.Embed.PaginatedEmbed;
using Grillbot.Services;
using Grillbot.Services.TempUnverify;
using Grillbot.Services.Unverify;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Modules
{
    [Group("unverify")]
    [Name("Odebrání přístupu")]
    [ModuleID("UnverifyModule")]
    public class UnverifyModule : BotModuleBase
    {
        private TempUnverifyService UnverifyService { get; }
        private UnverifyService Service { get; }

        public UnverifyModule(TempUnverifyService unverifyService, PaginationService paginationService, UnverifyService service)
            : base(paginationService: paginationService)
        {
            UnverifyService = unverifyService;
            Service = service;
        }

        [Command("")]
        [Summary("Dočasné odebrání rolí.")]
        [Remarks("Parametr time je ve formátu {cas}{m/h/d/M/y}, případně v ISO 8601. Např.: 30m, nebo `2020-08-17T23:59:59`.\nPopis: **m**: minuty, **h**: hodiny, **d**: dny, **M**: měsíce, **y**: roky.\n" +
            "Dále je důvod, proč daná osoba přišla o přístup\n\nCelý příkaz je pak vypadá např.:\n`{prefix}unverify 30m Přišel jsi o přístup @User1#1234 @User2#1354 ...`")]
        public async Task SetUnverifyAsync(string time, [Remainder] string reasonAndUserMentions = null)
        {
            try
            {
                if (await SetUnverifyRoutingAsync(time, reasonAndUserMentions))
                    return;

                var usersToUnverify = Context.Message.MentionedUsers.OfType<SocketGuildUser>().ToList();

                if (usersToUnverify.Count == 0)
                    return;

                var messages = await Service.SetUnverifyAsync(usersToUnverify, time, reasonAndUserMentions, Context.Guild, Context.User);
                await ReplyChunkedAsync(messages, 1);
            }
            catch (Exception ex)
            {
                if (ex is ValidationException || ex is FormatException || ex is ArgumentException)
                {
                    await ReplyAsync(ex.Message);
                    return;
                }

                throw;
            }
        }

        private async Task<bool> SetUnverifyRoutingAsync(string route, string parameters)
        {
            // Simply hack, because command routing cannot distinguish between a parameter and a function.
            switch (route)
            {
                case "list":
                    await ListUnverifyAsync();
                    return true;
                case "remove":
                    if (string.IsNullOrEmpty(parameters)) throw new ThrowHelpException();
                    await RemoveUnverifyAsync(Convert.ToInt32(parameters.Split(' ')[0])).ConfigureAwait(false);
                    return true;
                case "update":
                    if (string.IsNullOrEmpty(parameters)) throw new ThrowHelpException();
                    var fields = parameters.Split(' ');
                    if (fields.Length < 2)
                    {
                        await ReplyAsync("Chybí parametry.");
                        return true;
                    }

                    await UpdateUnverifyAsync(Convert.ToInt32(fields[0]), fields[1]).ConfigureAwait(false);
                    return true;
                case "stats":
                    await StatsAsync();
                    return true;
            }

            return false;
        }

        [Command("remove")]
        [Summary("Předčasné vrácení rolí.")]
        public async Task RemoveUnverifyAsync(int id)
        {
            try
            {
                var message = await UnverifyService.ReturnAccessAsync(id, Context.User).ConfigureAwait(false);
                await ReplyAsync(message).ConfigureAwait(false);
            }
            catch (NotFoundException ex)
            {
                await ReplyAsync(ex.Message);
            }
        }

        [Command("list")]
        [Summary("Seznam všech lidí, co má dočasně odebrané role.")]
        public async Task ListUnverifyAsync()
        {
            var users = await UnverifyService.ListPersonsAsync(Context.Guild);

            if (users.Count == 0)
            {
                await ReplyAsync("Nikdo zatím nemá odebraný přístup.");
                return;
            }

            var pages = new List<PaginatedEmbedPage>();

            foreach (var user in users)
            {
                var page = new PaginatedEmbedPage($"**{user.Username}**");

                page.AddField(new EmbedFieldBuilder().WithName("ID").WithValue(user.ID));
                page.AddField(new EmbedFieldBuilder().WithName("Do kdy").WithValue(user.EndDateTime.ToLocaleDatetime()));
                page.AddField(new EmbedFieldBuilder().WithName("Role").WithValue(string.Join(", ", user.Roles)));
                page.AddField(new EmbedFieldBuilder().WithName("Extra kanály").WithValue(user.ChannelOverrideList));
                page.AddField(new EmbedFieldBuilder().WithName("Důvod").WithValue(user.Reason));

                pages.Add(page);
            }

            var embed = new PaginatedEmbed()
            {
                Title = "Seznam osob s odebraným přístupem",
                Pages = pages,
                ResponseFor = Context.User,
                Thumbnail = Context.Client.CurrentUser.GetUserAvatarUrl()
            };

            await SendPaginatedEmbedAsync(embed);
        }

        [Command("update")]
        [Summary("Aktualizace času u záznamu o dočasném odebrání rolí.")]
        public async Task UpdateUnverifyAsync(int id, string time)
        {
            try
            {
                var message = await UnverifyService.UpdateUnverifyAsync(id, time, Context.User).ConfigureAwait(false);
                await ReplyAsync(message).ConfigureAwait(false);
            }
            catch (NotFoundException ex)
            {
                await ReplyAsync(ex.Message);
            }
        }

        [Command("stats")]
        [Summary("Statistiky unverify")]
        public async Task StatsAsync()
        {
            var users = await UnverifyService.ListPersonsAsync(Context.Guild);

            var embed = new BotEmbed(Context.User, title: "Statistiky unverify")
                .AddField("SelfUnverify", users.Count(o => o.IsSelfUnverify).FormatWithSpaces(), true)
                .AddField("Unverify", users.Count(o => !o.IsSelfUnverify).FormatWithSpaces(), true)
                .AddField("Celkem", users.Count.FormatWithSpaces(), true);

            await ReplyAsync(embed: embed.Build());
        }

        protected override void AfterExecute(CommandInfo command)
        {
            Service.Dispose();

            base.AfterExecute(command);
        }
    }
}
