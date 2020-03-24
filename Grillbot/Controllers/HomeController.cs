using System.Threading.Tasks;
using Grillbot.Models.BotStatus;
using Grillbot.Services;
using Grillbot.Services.Statistics;
using Microsoft.AspNetCore.Mvc;

namespace Grillbot.Controllers
{
    [Route("/")]
    public class HomeController : Controller
    {
        private BotStatusService StatusService { get; }
        private CalledEventStats CalledEventStats { get; }

        public HomeController(BotStatusService service, CalledEventStats calledEventStats)
        {
            StatusService = service;
            CalledEventStats = calledEventStats;
        }

        public async Task<IActionResult> Index()
        {
            var dbStatus = await StatusService.GetDbReport();

            var data = new WebStatus()
            {
                Simple = StatusService.GetSimpleStatus(),
                CallStats = StatusService.GetCallStats(),
                DBStatus = dbStatus,
                LoggerStats = StatusService.GetLoggerStats(),
                CalledEventStats = CalledEventStats.ToFormatedDictionary()
            };

            return View(data);
        }

        [Route("callStats")]
        public async Task<IActionResult> CallStats()
        {
            var data = CalledEventStats.GetSummarizedStats();

            return View(data);
        }
    }
}