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
    public class InviteController : Controller
    {
        private InviteTrackerService InviteTrackerService { get; }
        private DiscordSocketClient DiscordClient { get; }

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
            var pagination = await InviteTrackerService.GetPaginationInfoAsync(filter);

            return View(new InvitesListViewModel(DiscordClient.Guilds.ToList(), invitesFromGuild, filter, pagination));
        }
    }
}
