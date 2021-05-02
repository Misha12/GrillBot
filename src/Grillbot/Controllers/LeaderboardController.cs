using System.Threading.Tasks;
using Discord.WebSocket;
using Grillbot.Enums;
using Grillbot.Extensions.Discord;
using Grillbot.Models.Channelboard;
using Grillbot.Models.Unverify;
using Grillbot.Services.Channelboard;
using Grillbot.Services.Unverify;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Grillbot.Controllers
{
    [Authorize]
    [Route("leaderboard")]
    public class LeaderboardController : Controller
    {
        private ChannelStats ChannelStats { get; }
        private DiscordSocketClient Client { get; }
        private ChannelboardWeb ChannelboardWeb { get; }
        private UnverifyLogger UnverifyLogger { get; }

        public LeaderboardController(ChannelStats stats, DiscordSocketClient client, ChannelboardWeb channelboardWeb,
            UnverifyLogger unverifyLogger)
        {
            ChannelStats = stats;
            Client = client;
            ChannelboardWeb = channelboardWeb;
            UnverifyLogger = unverifyLogger;
        }

        [AllowAnonymous]
        [HttpGet("channels")]
        public async Task<IActionResult> ChannelsAsync([FromQuery] string key)
        {
            var item = ChannelboardWeb.GetItem(key);
            if (item == null)
                return View(new ChannelboardViewModel(LeaderboardErrors.InvalidKey));

            var guild = Client.GetGuild(item.GuildID);
            if (guild == null)
                return View(new ChannelboardViewModel(LeaderboardErrors.InvalidGuild));

            var user = await guild.GetUserFromGuildAsync(item.UserID);

            if (user == null)
                return View(new ChannelboardViewModel(LeaderboardErrors.UserAtGuildNotFound));

            var data = await ChannelStats.GetChannelboardDataAsync(guild, user);
            return View(new ChannelboardViewModel(guild, user, data));
        }

        [AllowAnonymous]
        [HttpGet("unverify")]
        public async Task<IActionResult> UnverifyAsync([FromQuery] ulong guildId)
        {
            var guild = Client.GetGuild(guildId);
            if (guild == null)
                return View(new UnverifyLeaderboardViewModel(LeaderboardErrors.InvalidGuild));

            var data = await UnverifyLogger.GetUnverifyStatisticsAsync(guild);
            return View(new UnverifyLeaderboardViewModel(guild, data));
        }
    }
}