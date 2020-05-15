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

        [HttpGet]
        public IActionResult Cache()
        {
            var cacheStatus = BotStatus.GetCacheStatus();

            var viewModel = new CacheStatusViewModel
            {
                CacheData = cacheStatus
            };

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