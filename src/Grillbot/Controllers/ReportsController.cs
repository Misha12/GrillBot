using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Grillbot.Models.BotStatus;
using Grillbot.Services;
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

        public ReportsController(BotStatusService statusService, InternalStatistics internalStatistics, ApiStatistics apiStatistics,
            DiscordSocketClient discordClient, IHostApplicationLifetime applicationLifetime)
        {
            StatusService = statusService;
            InternalStatistics = internalStatistics;
            ApiStatistics = apiStatistics;
            DiscordClient = discordClient;
            ApplicationLifetime = applicationLifetime;
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
                Latency = DiscordClient.Latency
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
    }
}
