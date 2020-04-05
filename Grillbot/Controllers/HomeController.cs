using System.Threading.Tasks;
using Grillbot.Models.BotStatus;
using Grillbot.Models.InMemoryLogger;
using Grillbot.Services;
using Grillbot.Services.InMemoryLogger;
using Grillbot.Services.Statistics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Grillbot.Controllers
{
    [Authorize]
    [Route("/")]
    public class HomeController : Controller
    {
        private BotStatusService StatusService { get; }
        private CalledEventStats CalledEventStats { get; }
        private InMemoryLoggerService LoggerService { get; }

        public HomeController(BotStatusService service, CalledEventStats calledEventStats, InMemoryLoggerService loggerService)
        {
            StatusService = service;
            CalledEventStats = calledEventStats;
            LoggerService = loggerService;
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
            return View(CalledEventStats.GetSummarizedStats());
        }

        [Route("logging")]
        public IActionResult Logging(LogLevel minLevel = LogLevel.Information, string section = null)
        {
            var entries = LoggerService.GetLogEntries(minLevel, section);
            return View(new LoggingViewModel(entries, minLevel, section));
        }
    }
}