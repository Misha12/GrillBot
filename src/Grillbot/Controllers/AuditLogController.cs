using Discord.WebSocket;
using Grillbot.Models.Audit;
using Grillbot.Services.Audit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Controllers
{
    [Authorize]
    [Route("Admin/Audit")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class AuditLogController : Controller
    {
        private AuditService AuditService { get; }
        private DiscordSocketClient DiscordClient { get; }

        public AuditLogController(AuditService auditService, DiscordSocketClient discordClient)
        {
            AuditService = auditService;
            DiscordClient = discordClient;
        }

        public async Task<IActionResult> IndexAsync(LogsFilter filter = null)
        {
            if (filter == null)
                filter = new LogsFilter();
            if (filter.GuildId == 0)
                filter.GuildId = DiscordClient.Guilds.FirstOrDefault()?.Id ?? 0;

            var logs = await AuditService.GetAuditLogsAsync(filter);
            var pagination = await AuditService.GetPaginationInfoAsync(filter);

            var viewModel = new AuditViewModel(logs, filter, pagination, DiscordClient.Guilds.ToList());
            return View(viewModel);
        }

        [HttpGet("Delete/{id}")]
        public async Task<IActionResult> DeleteRecordAsync(long id)
        {
            await AuditService.DeleteItemAsync(id);
            return RedirectToAction("Index");
        }
    }
}
