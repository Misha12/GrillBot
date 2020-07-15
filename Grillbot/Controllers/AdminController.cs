using System.Collections.Generic;
using Discord.WebSocket;
using Grillbot.Database.Repository;
using Grillbot.Models.CallStats;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Grillbot.Controllers
{
    [Authorize]
    [Route("/")]
    [Route("Admin")]
    public class AdminController : Controller
    {
        private DiscordSocketClient DiscordClient { get; }
        private ConfigRepository ConfigRepository { get; }

        public AdminController(ConfigRepository configRepository, DiscordSocketClient discordSocket)
        {
            ConfigRepository = configRepository;
            DiscordClient = discordSocket;
        }

        [HttpGet]
        public IActionResult Index()
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

        protected override void Dispose(bool disposing)
        {
            if(disposing)
            {
                ConfigRepository.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}