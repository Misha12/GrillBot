using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Grillbot.Models.BotStatus;
using Grillbot.Services;
using Grillbot.Services.Audit;
using Grillbot.Services.BackgroundTasks;
using Grillbot.Services.Statistics;
using Grillbot.Services.Statistics.ApiStats;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace Grillbot.Controllers
{
    [Authorize]
    [Route("Admin/Reports")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ReportsController : Controller
    {
        private BotStatusService StatusService { get; }
        private InternalStatistics InternalStatistics { get; }
        private ApiStatistics ApiStatistics { get; }
        private DiscordSocketClient DiscordClient { get; }
        private IHostApplicationLifetime ApplicationLifetime { get; }
        private BackgroundTaskQueue BackgroundTaskQueue { get; }
        private AuditService AuditService { get; }

        public ReportsController(BotStatusService statusService, InternalStatistics internalStatistics, ApiStatistics apiStatistics,
            DiscordSocketClient discordClient, IHostApplicationLifetime applicationLifetime, BackgroundTaskQueue backgroundTaskQueue,
            AuditService auditService)
        {
            StatusService = statusService;
            InternalStatistics = internalStatistics;
            ApiStatistics = apiStatistics;
            DiscordClient = discordClient;
            ApplicationLifetime = applicationLifetime;
            BackgroundTaskQueue = backgroundTaskQueue;
            AuditService = auditService;
        }

        public async Task<IActionResult> IndexAsync()
        {
            var getDbStatusTask = StatusService.GetDbReport();

            var result = new ReportsViewModel()
            {
                BotStatus = StatusService.GetSimpleStatus(),
                Commands = InternalStatistics.GetCommands(),
                Events = InternalStatistics.GetEvents(),
                GCMemoryInfo = GC.GetGCMemoryInfo(),
                Api = ApiStatistics.Data.FindAll(o => o.Count > 0),
                LoginState = DiscordClient.LoginState,
                ConnectionState = DiscordClient.ConnectionState,
                Latency = DiscordClient.Latency,
                BackgroundTasks = BackgroundTaskQueue.GetStatus()
            };

            result.Database = await getDbStatusTask;
            return View(result);
        }

        [HttpGet("shutdown")]
        public IActionResult ShutDown()
        {
            Task.Run(() =>
            {
                Task.WaitAll(Task.Delay(500));
                ApplicationLifetime.StopApplication();
            });

            return Redirect("https://google.com");
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
    }
}
