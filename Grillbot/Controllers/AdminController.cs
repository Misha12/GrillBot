using System.Threading.Tasks;
using Discord.WebSocket;
using Grillbot.Database.Repository;
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
    public class AdminController : Controller
    {
        private BotStatusService StatusService { get; }
        private InMemoryLoggerService LoggerService { get; }
        private InternalStatistics InternalStatistics { get; }
        private LogRepository LogRepository { get; }
        private DiscordSocketClient DiscordClient { get; }

        public AdminController(BotStatusService service, InMemoryLoggerService loggerService,
            InternalStatistics internalStatistics, LogRepository logRepository, DiscordSocketClient discordSocket)
        {
            StatusService = service;
            LoggerService = loggerService;
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
                ExecutedCommands = InternalStatistics.Commands,
                DBStatus = dbStatus,
                LoggerStats = StatusService.GetLoggerStats(),
                TriggeredEvents = InternalStatistics.Events
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

        [Route("Logging")]
        public IActionResult Logging(LogLevel minLevel = LogLevel.Information, string section = null)
        {
            var entries = LoggerService.GetLogEntries(minLevel, section);
            return View(new LoggingViewModel(entries, minLevel, section));
        }
    }
}