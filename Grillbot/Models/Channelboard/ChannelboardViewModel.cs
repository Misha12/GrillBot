using Discord.WebSocket;
using System.Collections.Generic;

namespace Grillbot.Models.Channelboard
{
    public class ChannelboardViewModel
    {
        public List<ChannelStatItem> Items { get; set; }
        public ChannelboardWebGuild Guild { get; set; }
        public ChannelboardWebUser User { get; set; }
        public ChannelboardErrors Error { get; set; }

        public ChannelboardViewModel(ChannelboardErrors error)
        {
            Error = error;
        }

        public ChannelboardViewModel(SocketGuild guild, SocketGuildUser user, List<ChannelStatItem> items)
        {
            Guild = ChannelboardWebGuild.Create(guild);
            User = ChannelboardWebUser.Create(user);
            Items = items;
        }
    }
}
