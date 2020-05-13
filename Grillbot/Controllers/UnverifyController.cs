using Discord.WebSocket;
using Grillbot.Extensions;
using Grillbot.Extensions.Discord;
using Grillbot.Models.TempUnverify.Admin;
using Grillbot.Services.TempUnverify;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;
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

        public UnverifyController(TempUnverifyLogService tempUnverifyLogService, TempUnverifyService tempUnverifyService, DiscordSocketClient discordClient)
        {
            TempUnverifyService = tempUnverifyService;
            TempUnverifyLogService = tempUnverifyLogService;
            DiscordClient = discordClient;
        }

        [HttpGet]
        public async Task<IActionResult> IndexAsync()
        {
            var unverified = await TempUnverifyService.ListPersonsAsync(null);
            return View(new UnverifyCurrentStatusViewModel(unverified));
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
            if(disposing)
            {
                TempUnverifyLogService.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}