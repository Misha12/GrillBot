using System.Threading.Tasks;
using Discord.WebSocket;
using Grillbot.Extensions.Discord;
using Grillbot.Models;
using Grillbot.Models.Channelboard;
using Grillbot.Services.Channelboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Grillbot.Controllers
{
    [Authorize]
    [Route("channelboard")]
    public class ChannelboardController : Controller
    {
        private ChannelStats ChannelStats { get; }
        private DiscordSocketClient Client { get; }
        private ChannelboardWeb ChannelboardWeb { get; }

        public ChannelboardController(ChannelStats stats, DiscordSocketClient client, ChannelboardWeb channelboardWeb)
        {
            ChannelStats = stats;
            Client = client;
            ChannelboardWeb = channelboardWeb;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index([FromQuery] string key)
        {
            var item = ChannelboardWeb.GetItem(key);
            if (item == null)
                return View(new ChannelboardViewModel() { Error = ChannelboardErrors.InvalidKey });

            var guild = Client.GetGuild(item.GuildID);
            if (guild == null)
                return View(new ChannelboardViewModel() { Error = ChannelboardErrors.InvalidGuild });

            var user = await guild.GetUserFromGuildAsync(item.UserID);

            if (user == null)
                return View(new ChannelboardViewModel() { Error = ChannelboardErrors.UserAtGuildNotFound });

            var data = await ChannelStats.GetChannelboardDataAsync(guild, user);

            var channelboard = new ChannelboardViewModel()
            {
                Guild = GuildInfo.Create(guild),
                Items = data,
                User = GuildUser.Create(user)
            };

            return View(channelboard);
        }
    }
}