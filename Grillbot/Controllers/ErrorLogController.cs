using System.Threading.Tasks;
using Grillbot.Database.Repository;
using Grillbot.Models.ErrorLog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Grillbot.Controllers
{
    [Authorize]
    [Route("Admin/ErrorLog")]
    public class ErrorLogController : Controller
    {
        private ErrorLogRepository ErrorLogRepository { get; }

        public ErrorLogController(ErrorLogRepository errorLogRepository)
        {
            ErrorLogRepository = errorLogRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index(long? id = null)
        {
            if (id == null)
                return View(new ErrorLogViewModel { ID = id });

            var logItem = await ErrorLogRepository.FindLogByIDAsync(id.Value);

            var viewModel = new ErrorLogViewModel()
            {
                Found = logItem != null,
                ID = id,
                LogItem = logItem
            };

            return View(viewModel);
        }

        protected override void Dispose(bool disposing)
        {
            ErrorLogRepository.Dispose();

            base.Dispose(disposing);
        }
    }
}
