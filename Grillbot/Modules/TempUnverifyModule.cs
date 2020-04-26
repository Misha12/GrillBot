using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Grillbot.Exceptions;
using Grillbot.Extensions;
using Grillbot.Extensions.Discord;
using Grillbot.Models.Embed;
using Grillbot.Services.Preconditions;
using Grillbot.Services.TempUnverify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Modules
{
    [Group("unverify")]
    [RequirePermissions]
    [Name("Odebrání přístupu")]
    public class TempUnverifyModule : BotModuleBase
    {
        private TempUnverifyService UnverifyService { get; }

        public TempUnverifyModule(TempUnverifyService unverifyService)
        {
            UnverifyService = unverifyService;
        }

        [Command("")]
        [Summary("Dočasné odebrání rolí.")]
        [Remarks("Parametr time je ve formátu {cas}{m/h/d}. Např.: 30m.\nPopis: m: minuty, h: hodiny, d: dny.\n" +
            "Dále je důvod, proč daná osoba přišla o role. A nakonec seznam (mentions) uživatelů.\n" +
            "Celý příkaz je pak vypadá např.:\n{prefix}unverify 30m Přišel jsi o role @User1#1234 @User2#1354 ...")]
        public async Task SetUnverifyAsync(string time, [Remainder] string reasonAndUserMentions = null)
        {
            // Simply hack, because command routing cannot distinguish between a parameter and a function.
            switch (time)
            {
                case "list":
                    await ListUnverifyAsync().ConfigureAwait(false);
                    return;
                case "remove":
                    if (string.IsNullOrEmpty(reasonAndUserMentions)) throw new ThrowHelpException();
                    await RemoveUnverifyAsync(Convert.ToInt32(reasonAndUserMentions.Split(' ')[0])).ConfigureAwait(false);
                    return;
                case "update":
                    if (string.IsNullOrEmpty(reasonAndUserMentions)) throw new ThrowHelpException();
                    var fields = reasonAndUserMentions.Split(' ');
                    if (fields.Length < 2)
                    {
                        await ReplyAsync("Chybí parametry.").ConfigureAwait(false);
                        return;
                    }
                    await UpdateUnverifyAsync(Convert.ToInt32(fields[0]), fields[1]).ConfigureAwait(false);
                    return;
            }

            var usersToUnverify = Context.Message.MentionedUsers.OfType<SocketGuildUser>().ToList();

            if (usersToUnverify.Count > 0)
            {
                var message = await UnverifyService.RemoveAccessAsync(usersToUnverify, time,
                    reasonAndUserMentions, Context.Guild, Context.User).ConfigureAwait(false);
                await ReplyAsync(message).ConfigureAwait(false);
            }
        }

        [Command("remove")]
        [Summary("Předčasné vrácení rolí.")]
        public async Task RemoveUnverifyAsync(int id)
        {
            var message = await UnverifyService.ReturnAccessAsync(id, Context.User).ConfigureAwait(false);
            await ReplyAsync(message).ConfigureAwait(false);
        }

        [Command("list")]
        [Summary("Seznam všech lidí, co má dočasně odebrané role.")]
        public async Task ListUnverifyAsync()
        {
            var users = await UnverifyService.ListPersonsAsync(Context.Guild);

            var template = new BotEmbed(Context.Message.Author, title: "Seznam osob s odebraným přístupem", thumbnail: Context.Client.CurrentUser.GetUserAvatarUrl());
            var fields = new List<EmbedFieldBuilder>();

            foreach (var user in users)
            {
                var desc = string.Join("\n", new[]
                {
                    $"ID: {user.ID}",
                    $"Do kdy: {user.EndDateTime.ToLocaleDatetime()}",
                    $"Role: {string.Join(", ", user.Roles)}",
                    $"Extra kanály: {user.ChannelOverrideList}",
                    $"Důvod: {user.Reason}"
                });

                var field = new EmbedFieldBuilder()
                    .WithName(user.Username)
                    .WithValue(desc);

                fields.Add(field);
            }

            await ReplyChunkedAsync(fields, template, 10);
        }

        [Command("update")]
        [Summary("Aktualizace času u záznamu o dočasném odebrání rolí.")]
        public async Task UpdateUnverifyAsync(int id, string time)
        {
            var message = await UnverifyService.UpdateUnverifyAsync(id, time, Context.User).ConfigureAwait(false);
            await ReplyAsync(message).ConfigureAwait(false);
        }
    }
}
