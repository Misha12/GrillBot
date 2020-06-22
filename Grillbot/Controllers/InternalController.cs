using Grillbot.Models.Internal;
using Grillbot.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Grillbot.Controllers
{
    [Authorize]
    [Route("Admin/Internal")]
    public class InternalController : Controller
    {
        private BotStatusService BotStatus { get; }

        public InternalController(BotStatusService botStatusService)
        {
            BotStatus = botStatusService;
        }

        [HttpGet("Cache")]
        public IActionResult Cache()
        {
            var viewModel = new CacheStatusViewModel { CacheData = BotStatus.GetCacheStatus() };
            return View(viewModel);
        }

        [HttpGet("Memory")]
        public IActionResult Memory()
        {
            var viewModel = new MemoryStatusViewModel();
            return View(viewModel);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                BotStatus.Dispose();

            base.Dispose(disposing);
        }
    }
}