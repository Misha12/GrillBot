﻿using Discord;
using Discord.WebSocket;
using Grillbot.Database.Entity.Users;
using System;

namespace Grillbot.Models.Channelboard
{
    public class ChannelStatItem
    {
        public IChannel Channel { get; set; }
        public long Count { get; set; }
        public DateTime LastMessageAt { get; set; }

        public SocketTextChannel GuildChannel => Channel is SocketTextChannel channel ? channel : null;

        public ChannelStatItem(IChannel channel, UserChannel userChannel)
        {
            Count = userChannel.Count;
            LastMessageAt = userChannel.LastMessageAt;
            Channel = channel;
        }
    }
}
