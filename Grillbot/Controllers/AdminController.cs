using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Grillbot.Database.Repository;
using Grillbot.Extensions;
using Grillbot.Models.BotStatus;
using Grillbot.Models.CallStats;
using Grillbot.Models.Math;
using Grillbot.Models.TeamSearch;
using Grillbot.Models.TempUnverify.Admin;
using Grillbot.Services;
using Grillbot.Services.Math;
using Grillbot.Services.Statistics;
using Grillbot.Services.TeamSearch;
using Grillbot.Services.TempUnverify;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Grillbot.Controllers
{
    [Authorize]
    [Route("/")]
    [Route("Admin")]
    public class AdminController : Controller
    {
        private BotStatusService StatusService { get; }
        private InternalStatistics InternalStatistics { get; }
        private DiscordSocketClient DiscordClient { get; }
        private ConfigRepository ConfigRepository { get; }
        private TeamSearchService TeamSearchService { get; }
        private MathService MathService { get; }
        private TempUnverifyService TempUnverifyService { get; }
        private TempUnverifyLogService TempUnverifyLogService { get; }

        public AdminController(BotStatusService service, InternalStatistics internalStatistics, ConfigRepository configRepository, DiscordSocketClient discordSocket,
            TeamSearchService teamSearchService, MathService mathService, TempUnverifyService tempUnverifyService, TempUnverifyLogService tempUnverifyLogService)
        {
            StatusService = service;
            InternalStatistics = internalStatistics;
            ConfigRepository = configRepository;
            DiscordClient = discordSocket;
            TeamSearchService = teamSearchService;
            MathService = mathService;
            TempUnverifyService = tempUnverifyService;
            TempUnverifyLogService = tempUnverifyLogService;
        }

        [HttpGet]
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

        [HttpGet("CallStats")]
        public IActionResult CallStats()
        {
            var configs = ConfigRepository.GetAllConfigurations();
            var commands = new List<CommandStatSummaryItem>();

            foreach (var config in configs)
            {
                var guild = DiscordClient.GetGuild(config.GuildIDSnowflake);

                commands.Add(new CommandStatSummaryItem()
                {
                    CallsCount = config.UsedCount,
                    Command = config.Command,
                    Group = config.Group,
                    Guild = guild == null ? $"UnknownGuild ({config.GuildIDSnowflake})" : $"{guild.Name}",
                    PermissionsCount = config.Permissions.Count
                });
            }

            return View(new CallStatsViewModel(commands));
        }

        [HttpGet("Unverify")]
        public async Task<IActionResult> UnverifyAsync()
        {
            var viewModel = new UnverifyViewModel()
            {
                CurrentUnverified = await TempUnverifyService.ListPersonsForAdminAsync(),
                Guilds = DiscordClient.Guilds.ToList(),
                Request = new UnverifyAuditFilterRequest(),
                Users = DiscordClient.Guilds.SelectMany(o => o.Users.ToList()).DistinctBy(o => o.Id).ToList(),
                LogItems = await TempUnverifyLogService.GetAuditLogAsync(new UnverifyAuditFilterRequest())
            };

            return View(viewModel);
        }

        [HttpPost("Unverify")]
        public async Task<IActionResult> UnverifyAsync(UnverifyAuditFilterRequest request)
        {
            var viewModel = new UnverifyViewModel()
            {
                CurrentUnverified = await TempUnverifyService.ListPersonsForAdminAsync(),
                Guilds = DiscordClient.Guilds.ToList(),
                Request = request,
                Users = DiscordClient.Guilds.SelectMany(o => o.Users.ToList()).DistinctBy(o => o.Id).ToList(),
                LogItems = await TempUnverifyLogService.GetAuditLogAsync(request)
            };

            return View();
        }

        [HttpGet("TeamSearch")]
        public async Task<IActionResult> TeamSearchAsync()
        {
            var items = await TeamSearchService.GetAllItemsAsync();
            return View(new TeamSearchViewModel(items));
        }

        [HttpGet("Math")]
        public IActionResult Math()
        {
            return View(new MathViewModel(MathService.Sessions));
        }
    }
}