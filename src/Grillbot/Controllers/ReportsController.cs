using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Grillbot.Models.BotStatus;
using Grillbot.Services;
using Grillbot.Services.Audit;
using Grillbot.Services.BackgroundTasks;
using Grillbot.Services.Statistics;
using Grillbot.Services.UserManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Grillbot.Controllers
{
    [Authorize]
    [Route("Admin/Reports")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ReportsController : Controller
    {
        private BotStatusService StatusService { get; }
        private InternalStatistics InternalStatistics { get; }
        private DiscordSocketClient DiscordClient { get; }
        private BackgroundTaskQueue BackgroundTaskQueue { get; }
        private AuditService AuditService { get; }
        private UserService UserService { get; }

        public ReportsController(BotStatusService statusService, InternalStatistics internalStatistics, DiscordSocketClient discordClient,
            BackgroundTaskQueue backgroundTaskQueue, AuditService auditService, UserService userService)
        {
            StatusService = statusService;
            InternalStatistics = internalStatistics;
            DiscordClient = discordClient;
            BackgroundTaskQueue = backgroundTaskQueue;
            AuditService = auditService;
            UserService = userService;
        }

        public async Task<IActionResult> IndexAsync()
        {
            var getDbStatusTask = StatusService.GetDbReport();
            var getCommandsReportTask = StatusService.GetCommandsReportAsync();

            var result = new ReportsViewModel()
            {
                BotStatus = StatusService.GetSimpleStatus(),
                Events = InternalStatistics.GetEvents(),
                GCMemoryInfo = GC.GetGCMemoryInfo(),
                LoginState = DiscordClient.LoginState,
                ConnectionState = DiscordClient.ConnectionState,
                Latency = DiscordClient.Latency,
                BackgroundTasks = BackgroundTaskQueue.GetStatus()
            };

            result.Database = await getDbStatusTask;
            result.Commands = await getCommandsReportTask;
            return View(result);
        }

        [HttpGet("AuditLog")]
        public async Task<IActionResult> AuditLogAsync()
        {
            var perTypeStats = await AuditService.GetStatisticsPerType();

            var viewModel = new AuditLogReportsViewModel();
            foreach (var stat in perTypeStats)
                viewModel.PerTypeStats[stat.Key] = stat.Value;

            return View(viewModel);
        }

        [HttpGet("WebStats")]
        public async Task<IActionResult> WebStatsAsync()
        {
            var statistics = await UserService.GetWebStatsAsync();
            var viewModel = new WebStatisticsViewModel(statistics);

            return View(viewModel);
        }

        [HttpGet("BackgroundTasks")]
        public async Task<IActionResult> BackgroundTasksAsync()
        {
            var data = BackgroundTaskQueue.GetGroupedSerializedList();
            var viewModel = new BackgroundTasksReportViewModel(data);

            return View(viewModel);
        }
    }
}
