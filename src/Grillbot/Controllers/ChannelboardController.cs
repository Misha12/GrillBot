﻿using System.Threading.Tasks;
using Discord.WebSocket;
using Grillbot.Enums;
using Grillbot.Extensions.Discord;
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
                return View(new ChannelboardViewModel(ChannelboardErrors.InvalidKey));

            var guild = Client.GetGuild(item.GuildID);
            if (guild == null)
                return View(new ChannelboardViewModel(ChannelboardErrors.InvalidGuild));

            var user = await guild.GetUserFromGuildAsync(item.UserID);

            if (user == null)
                return View(new ChannelboardViewModel(ChannelboardErrors.UserAtGuildNotFound));

            var data = await ChannelStats.GetChannelboardDataAsync(guild, user);
            return View(new ChannelboardViewModel(guild, user, data));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                ChannelboardWeb.Dispose();

            base.Dispose(disposing);
        }
    }
}