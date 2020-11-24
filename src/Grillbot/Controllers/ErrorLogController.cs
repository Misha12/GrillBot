using System.Linq;
using System.Threading.Tasks;
using Grillbot.Database;
using Grillbot.Database.Entity;
using Grillbot.Models.ErrorLog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Grillbot.Controllers
{
    [Authorize]
    [Route("Admin/ErrorLog")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ErrorLogController : Controller
    {
        private IGrillBotRepository GrillBotRepository { get; }

        public ErrorLogController(IGrillBotRepository grillBotRepository)
        {
            GrillBotRepository = grillBotRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index(long? id = null)
        {
            const int tableDataLength = 90;
            var logs = await GrillBotRepository.ErrorLogRepository.GetLastLogs(25).Select(o => new ErrorLogItem()
            {
                CreatedAt = o.CreatedAt,
                ID = o.ID,
                Data = o.Data.Length > tableDataLength ? o.Data.Substring(0, tableDataLength - 3) + "..." : o.Data
            }).ToListAsync();

            if (id == null)
                return View(new ErrorLogViewModel(null, logs, false));

            var logItem = await GrillBotRepository.ErrorLogRepository.FindLogByIDAsync(id.Value);
            return View(new ErrorLogViewModel(logItem, logs, true));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> RemoveItem(long id)
        {
            var item = await GrillBotRepository.ErrorLogRepository.FindLogByIDAsync(id);

            if (item == null)
                return RedirectToAction("Index");

            GrillBotRepository.Remove(item);
            await GrillBotRepository.CommitAsync();
            return RedirectToAction("Index");
        }
    }
}
