using Discord.WebSocket;
using Grillbot.Exceptions;
using Grillbot.Extensions.Discord;
using Grillbot.Models.Unverify;
using Grillbot.Services.Unverify;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Controllers
{
    [Authorize]
    [Route("/")]
    [Route("Admin")]
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
        public async Task<IActionResult> AuditAsync(UnverifyAuditFilterFormData formData = null)
        {
            if (formData == null)
                formData = new UnverifyAuditFilterFormData();

            if (formData.GuildID == 0)
                formData.GuildID = DiscordClient.Guilds.FirstOrDefault()?.Id ?? 0;

            if (formData.Page < 1)
                formData.Page = 1;

            var errorMessage = Request.Query.TryGetValue("ErrMsg", out var values) ? values.ToString() : null;
            var logs = await UnverifyService.UnverifyLogger.GetLogsAsync(formData);
            var pagination = await UnverifyService.UnverifyLogger.CreatePaginationInfo(formData);
            var viewModel = new UnverifyAuditViewModel(DiscordClient, logs, formData, pagination, errorMessage);

            return View(viewModel);
        }

        [HttpGet("Recover/{id}")]
        public async Task<IActionResult> RecoverAsync(long id)
        {
            try
            {
                var fromUser = await DiscordClient.GetUserFromClaimsAsync(User);
                await UnverifyService.RecoverToStateAsync(id, fromUser);

                return RedirectToAction("Audit");
            }
            catch (Exception ex)
            {
                if (ex is ValidationException || ex is NotFoundException)
                    return RedirectToAction("Audit", new { ErrMsg = ex.Message });

                throw;
            }
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