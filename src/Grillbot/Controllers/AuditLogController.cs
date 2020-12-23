using Discord.WebSocket;
using Grillbot.Models.Audit;
using Grillbot.Services.Audit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
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
        private FileExtensionContentTypeProvider ContentTypeProvider { get; }

        public AuditLogController(AuditService auditService, DiscordSocketClient discordClient, FileExtensionContentTypeProvider contentTypeProvider)
        {
            AuditService = auditService;
            DiscordClient = discordClient;
            ContentTypeProvider = contentTypeProvider;
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

        [HttpGet("DownloadFile")]
        public async Task<IActionResult> DownloadFileAsync([FromQuery] string filename)
        {
            var file = await AuditService.GetFileAsync(filename);

            if (file == null)
                return NotFound();

            var contentType = ContentTypeProvider.TryGetContentType(file.Filename, out string type) ? type : "application/octet-stream";
            return File(file.Content, contentType);
        }

        [HttpGet("Delete/{id}")]
        public async Task<IActionResult> DeleteRecordAsync(long id)
        {
            await AuditService.DeleteItemAsync(id);
            return RedirectToAction("Index");
        }
    }
}
