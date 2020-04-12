using System.Threading.Tasks;
using Discord.WebSocket;
using Grillbot.Database.Repository;
using Grillbot.Models.BotStatus;
using Grillbot.Services;
using Grillbot.Services.Statistics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Grillbot.Controllers
{
    [Authorize]
    [Route("/")]
    public class AdminController : Controller
    {
        private BotStatusService StatusService { get; }
        private InternalStatistics InternalStatistics { get; }
        private LogRepository LogRepository { get; }
        private DiscordSocketClient DiscordClient { get; }

        public AdminController(BotStatusService service, InternalStatistics internalStatistics, LogRepository logRepository, DiscordSocketClient discordSocket)
        {
            StatusService = service;
            InternalStatistics = internalStatistics;
            LogRepository = logRepository;
            DiscordClient = discordSocket;
        }
        
        public async Task<IActionResult> Index()
        {
            var dbStatus = await StatusService.GetDbReport();

            var data = new WebStatus()
            {
                Simple = StatusService.GetSimpleStatus(),
                ExecutedCommands = InternalStatistics.GetCommands(),
                DBStatus = dbStatus,
                LoggerStats = StatusService.GetLoggerStats(),
                TriggeredEvents = InternalStatistics.GetEvents()
            };

            return View(data);
        }

        [Route("CallStats")]
        public IActionResult CallStats()
        {
            var data = LogRepository.GetSummarizedCommandLog();

            foreach(var item in data)
            {
                item.ReplaceGuildNames(DiscordClient);
            }

            return View(data);
        }
    }
}