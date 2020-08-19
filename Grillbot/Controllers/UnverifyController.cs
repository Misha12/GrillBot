using Discord.WebSocket;
using Grillbot.Extensions.Discord;
using Grillbot.Models;
using Grillbot.Models.TempUnverify.Admin;
using Grillbot.Services.TempUnverify;
using Grillbot.Services.Unverify;
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
        private TempUnverifyLogService TempUnverifyLogService { get; }
        private DiscordSocketClient DiscordClient { get; }
        private UnverifyService UnverifyService { get; }

        public UnverifyController(TempUnverifyLogService tempUnverifyLogService, DiscordSocketClient discordClient, UnverifyService unverifyService)
        {
            TempUnverifyLogService = tempUnverifyLogService;
            DiscordClient = discordClient;
            UnverifyService = unverifyService;
        }

        [HttpGet]
        public async Task<IActionResult> IndexAsync()
        {
            var data = new List<UnverifyInfo>();

            foreach (var guild in DiscordClient.Guilds)
            {
                var unverifies = await UnverifyService.GetCurrentUnverifies(guild);
                data.AddRange(unverifies);
            }

            return View(new UnverifyCurrentStatusViewModel(data));
        }

        [HttpGet("RemoveAccess/{id}")]
        public async Task<IActionResult> RemoveAccessAsync(int id)
        {
            var loggedUser = await DiscordClient.GetUserFromClaimsAsync(User);
            await UnverifyService.RemoveUnverifyFromWebAsync(id, loggedUser);

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