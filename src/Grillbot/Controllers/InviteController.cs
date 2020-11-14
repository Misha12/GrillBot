using Discord.WebSocket;
using Grillbot.Models.Invites;
using Grillbot.Services.InviteTracker;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Controllers
{
    [Authorize]
    [Route("Admin/Invite")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class InviteController : Controller
    {
        private InviteTrackerService InviteTrackerService { get; set; }
        private DiscordSocketClient DiscordClient { get; set; }

        public InviteController(InviteTrackerService inviteTrackerService, DiscordSocketClient discordClient)
        {
            InviteTrackerService = inviteTrackerService;
            DiscordClient = discordClient;
        }

        [HttpGet]
        public async Task<IActionResult> IndexAsync(InvitesListFilter filter = null)
        {
            if (filter == null || filter.GuildID == 0)
                filter = InvitesListFilter.CreateDefault(DiscordClient);

            var invitesFromGuild = await InviteTrackerService.GetStoredInvitesAsync(filter);
            return View(new InvitesListViewModel(DiscordClient.Guilds.ToList(), invitesFromGuild, filter));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                InviteTrackerService.Dispose();

            base.Dispose(disposing);
        }
    }
}
