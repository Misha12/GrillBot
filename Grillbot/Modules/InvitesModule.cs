using Discord.Commands;
using Discord.WebSocket;
using Grillbot.Attributes;
using Grillbot.Exceptions;
using Grillbot.Extensions;
using Grillbot.Extensions.Discord;
using Grillbot.Services.InviteTracker;
using System;
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

            var message = users.Where(o => o?.User != null).Select(o => o.User.GetFullName());
            if (message.Count() > 25)
            {
                var fileContent = string.Join(Environment.NewLine, message);
                using var stream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));

                await Context.Channel.SendFileAsync(stream, $"{code}.txt");
                return;
            }

            await ReplyChunkedAsync(message.Select(o => $"> {o}"), 10);
        }

        [Command("list")]
        [Summary("Seznam všech pozvánek, které byly použity.")]
        public async Task ListInvitesAsync()
        {
            var invites = await InviteTracker.GetStoredInvitesAsync(Context.Guild);

            if (invites.Count == 0)
            {
                await ReplyAsync("Ještě nebyla použita žádná poznámka.");
                return;
            }

            var messages = invites.Select(o =>
            {
                if (o.Code == Context.Guild.VanityURLCode)
                    return $"> {o.Code,-15}{(o.Uses ?? 0).FormatWithSpaces()}";

                return $"> {o.Code,-15}{o.CreatedAt.Value.LocalDateTime.ToLocaleDatetime()}\t{o.Creator?.GetFullName() ?? "Neznámý uživatel"}\t{(o.Uses ?? 0).FormatWithSpaces()}";
            });

            await ReplyChunkedAsync(messages, 10);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                InviteTracker.Dispose();

            base.Dispose(disposing);
        }
    }
}
