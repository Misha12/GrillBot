using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Grillbot.Database.Repository;
using Grillbot.Models.BotStatus;
using Grillbot.Models.CallStats;
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
        private DiscordSocketClient DiscordClient { get; }
        private ConfigRepository ConfigRepository { get; }

        public AdminController(BotStatusService service, InternalStatistics internalStatistics, ConfigRepository configRepository, DiscordSocketClient discordSocket)
        {
            StatusService = service;
            InternalStatistics = internalStatistics;
            ConfigRepository = configRepository;
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
    }
}