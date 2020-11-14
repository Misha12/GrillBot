using Discord.Commands;
using Discord.WebSocket;
using Grillbot.Attributes;
using Grillbot.Exceptions;
using Grillbot.Services.InviteTracker;
using System;
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                InviteTracker.Dispose();

            base.Dispose(disposing);
        }
    }
}
