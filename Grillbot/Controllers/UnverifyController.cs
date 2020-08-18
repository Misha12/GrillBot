using Discord.WebSocket;
using Grillbot.Extensions.Discord;
using Grillbot.Models.TempUnverify.Admin;
using Grillbot.Services.TempUnverify;
using Grillbot.Services.Unverify;
using Grillbot.Services.Unverify.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grillbot.Controllers
{
    [Authorize]
    [Route("Admin/Unverify")]
    public class UnverifyController : Controller
    {
        private TempUnverifyService TempUnverifyService { get; }
        private TempUnverifyLogService TempUnverifyLogService { get; }
        private DiscordSocketClient DiscordClient { get; }
        private UnverifyService UnverifyService { get; }

        public UnverifyController(TempUnverifyLogService tempUnverifyLogService, TempUnverifyService tempUnverifyService, DiscordSocketClient discordClient,
            UnverifyService unverifyService)
        {
            TempUnverifyService = tempUnverifyService;
            TempUnverifyLogService = tempUnverifyLogService;
            DiscordClient = discordClient;
            UnverifyService = unverifyService;
        }

        [HttpGet]
        public async Task<IActionResult> IndexAsync()
        {
            var unverifiesData = new List<UnverifyUserProfile>();

            foreach (var guild in DiscordClient.Guilds)
            {
                var unverifies = await UnverifyService.GetCurrentUnverifies(guild);
                unverifiesData.AddRange(unverifies);
            }

            return View(new UnverifyCurrentStatusViewModel(unverifiesData));
        }

        [HttpGet("RemoveAccess/{id}")]
        public async Task<IActionResult> RemoveAccessAsync(int id)
        {
            var loggedUser = await DiscordClient.GetUserFromClaimsAsync(User);
            await TempUnverifyService.ReturnAccessAsync(id, loggedUser);

            return RedirectToAction("Index");
        }

        [HttpGet("Audit")]
        public async Task<IActionResult> AuditAsync()
        {
            var request = new UnverifyAuditFilterRequest();
            var logItems = await TempUnverifyLogService.GetAuditLogAsync(request);
            var viewModel = new UnverifyAuditViewModel(DiscordClient, logItems, request);

            return View(viewModel);
        }

        [HttpPost("Audit")]
        public async Task<IActionResult> AuditAsync([FromForm] UnverifyAuditFilterRequest request)
        {
            var logItems = await TempUnverifyLogService.GetAuditLogAsync(request);
            var viewModel = new UnverifyAuditViewModel(DiscordClient, logItems, request);

            return View(viewModel);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                TempUnverifyLogService.Dispose();
                UnverifyService.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}