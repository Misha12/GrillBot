using Discord;
using Grillbot.Database.Entity.Users;
using System;

namespace Grillbot.Models.Users
{
    public class WebAdminUserChannel
    {
        public IChannel Channel { get; set; }
        public long Count { get; set; }
        public DateTime LastMessageAt { get; set; }

        public WebAdminUserChannel(IChannel channel, UserChannel userChannel)
        {
            Count = userChannel.Count;
            LastMessageAt = userChannel.LastMessageAt;
            Channel = channel;
        }
    }
}
