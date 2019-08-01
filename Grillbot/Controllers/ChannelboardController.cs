using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Grillbot.Models;
using Grillbot.Services.Statistics;
using Microsoft.AspNetCore.Http;
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

            var guild = DiscordClient.Guilds.First();
            var data = new Channelboard()
            {
                Items = ChannelStats.GetChannelboardData(token, DiscordClient, out ChannelboardWebToken webToken),
                Guild = new GuildInfo()
                {
                    AvatarUrl = guild.IconUrl,
                    Name = guild.Name,
                    UsersCount = guild.Users.Count
                },
                StatsFor = GetUsername(guild.GetUser(webToken.UserID))
            };

            return Ok(data);
        }

        private string GetUsername(SocketGuildUser user)
        {
            var builder = new StringBuilder()
                .Append(user.Username);

            if (string.IsNullOrEmpty(user.Nickname))
                builder.Append("#").Append(user.Discriminator);
            else
                builder.Append(" (").Append(user.Nickname).Append("#").Append(user.Discriminator).Append(")");

            return builder.ToString();
        }
    }
}