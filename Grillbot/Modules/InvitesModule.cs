using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Grillbot.Attributes;
using Grillbot.Exceptions;
using Grillbot.Extensions.Discord;
using Grillbot.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grillbot.Modules
{
    [Group("invite")]
    [Name("Správa pozvánek")]
    [ModuleID("InvitesModule")]
    public class InvitesModule : BotModuleBase
    {
        private InviteTrackerService InviteTracker { get; }

        public InvitesModule(InviteTrackerService inviteTracker)
        {
            InviteTracker = inviteTracker;
        }

        [Command("assign")]
        [Summary("Přiřazení pozvánky.")]
        [Remarks("Pokud byl použit kód `vanity`, tak dojde k přiřazení vanity url.")]
        public async Task AssignInvite(string code, SocketUser toUser)
        {
            if (!string.IsNullOrEmpty(Context.Guild.VanityURLCode) && string.Equals(code, "vanity", StringComparison.InvariantCultureIgnoreCase))
                code = Context.Guild.VanityURLCode;

            try
            {
                await InviteTracker.AssignInviteToUserAsync(toUser, Context.Guild, code);
                await ReplyAsync("Pozvánka byla úspěšně přiřazena");
            }
            catch (NotFoundException ex)
            {
                await ReplyAsync(ex.Message);
            }
        }

        [Command("refresh")]
        [Summary("Aktualizace pozvánek v paměti.")]
        public async Task RefreshAsync()
        {
            var message = await InviteTracker.RefreshInvitesAsync();
            await ReplyAsync(message);
        }

        [Command("users")]
        [Summary("Získání seznamu uživatelů, kteří daný kód použili.")]
        public async Task GetUsersAsync(string code)
        {
            var users = await InviteTracker.GetUsersWithCodeAsync(Context.Guild, code);

            var message = users.Select(o => o.User.GetFullName());
            if (message.Count() > 25)
            {
                var fileContent = string.Join(Environment.NewLine, message);
                using var stream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));

                await Context.Channel.SendFileAsync(stream, $"{code}.txt");
                return;
            }

            await ReplyChunkedAsync(message.Select(o => $"> {o}"), 10);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                InviteTracker.Dispose();

            base.Dispose(disposing);
        }
    }
}
