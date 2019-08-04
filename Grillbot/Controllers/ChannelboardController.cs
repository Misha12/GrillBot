using System.Linq;
using Discord.WebSocket;
using Grillbot.Models;
using Grillbot.Services.Statistics;
using Microsoft.AspNetCore.Mvc;

namespace Grillbot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChannelboardController : ControllerBase
    {
        private ChannelStats ChannelStats { get; set; }
        private DiscordSocketClient DiscordClient { get; set; }

        public ChannelboardController(Statistics statistics, DiscordSocketClient discordClient)
        {
            ChannelStats = statistics.ChannelStats;
            DiscordClient = discordClient;
        }

        public IActionResult Index() => NoContent();

        [HttpGet("[action]")]
        public IActionResult GetChannelboardData(string token)
        {
            if (string.IsNullOrEmpty(token))
                return BadRequest(new { Error = "Missing token", Code = ChannelboardErrors.MissingWebToken });

            if (!ChannelStats.ExistsWebToken(token))
                return BadRequest(new { Error = "Invalid token", Code = ChannelboardErrors.InvalidWebToken });

            var channelboardData = ChannelStats.GetChannelboardData(token, DiscordClient, out ChannelboardWebToken webToken);
            var guild = DiscordClient.Guilds.First();
            var guildUser = guild.GetUser(webToken.UserID);

            var data = new Channelboard()
            {
                Items = channelboardData,
                Guild = GuildInfo.Create(guild),
                User = GuildUser.Create(guildUser)
            };

            return Ok(data);
        }
    }
}