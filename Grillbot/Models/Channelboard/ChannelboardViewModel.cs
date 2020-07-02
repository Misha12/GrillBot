using Discord.WebSocket;
using Grillbot.Enums;
using Grillbot.Models.Users;
using System.Collections.Generic;

namespace Grillbot.Models.Channelboard
{
    public class ChannelboardViewModel
    {
        public List<ChannelStatItem> Items { get; set; }
        public ChannelboardWebGuild Guild { get; set; }
        public SimpleUserInfo User { get; set; }
        public ChannelboardErrors Error { get; set; }

        public ChannelboardViewModel(ChannelboardErrors error)
        {
            Error = error;
        }

        public ChannelboardViewModel(SocketGuild guild, SocketGuildUser user, List<ChannelStatItem> items)
        {
            if (guild != null)
                Guild = ChannelboardWebGuild.Create(guild);

            if (user != null)
                User = SimpleUserInfo.Create(user);

            Items = items;
        }
    }
}
