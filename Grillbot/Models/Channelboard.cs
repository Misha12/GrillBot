﻿using Grillbot.Services.Statistics;
using System.Collections.Generic;

namespace Grillbot.Models
{
    public class Channelboard
    {
        public List<ChannelboardItem> Items { get; set; }
        public GuildInfo Guild { get; set; }
        public string StatsFor { get; set; }

        public Channelboard()
        {
            Items = new List<ChannelboardItem>();
            Guild = new GuildInfo();
        }
    }
}
