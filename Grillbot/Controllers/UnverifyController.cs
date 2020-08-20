using Discord.WebSocket;
using Grillbot.Extensions.Discord;
using Grillbot.Models.Unverify;
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
        private DiscordSocketClient DiscordClient { get; }
        private UnverifyService UnverifyService { get; }

        public UnverifyController(DiscordSocketClient discordClient, UnverifyService unverifyService)
        {
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
            var formData = new UnverifyAuditFilterFormData(DiscordClient);
            var logs = await UnverifyService.UnverifyLogger.GetLogsAsync(formData);
            var viewModel = new UnverifyAuditViewModel(DiscordClient, logs, formData);

            return View(viewModel);
        }

        [HttpPost("Audit")]
        public async Task<IActionResult> AuditAsync([FromForm] UnverifyAuditFilterFormData formData)
        {
            var logs = await UnverifyService.UnverifyLogger.GetLogsAsync(formData);
            var viewModel = new UnverifyAuditViewModel(DiscordClient, logs, formData);

            return View(viewModel);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                UnverifyService.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}