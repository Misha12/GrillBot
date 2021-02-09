using Discord.WebSocket;
using Grillbot.Enums;
using Grillbot.Models.Users;
using System.Collections.Generic;

namespace Grillbot.Models.Channelboard
{
    public class ChannelboardViewModel
    {
        public List<ChannelStatItem> Items { get; set; }
        public SocketGuild Guild { get; set; }
        public SimpleUserInfo User { get; set; }
        public LeaderboardErrors Error { get; set; }

        public ChannelboardViewModel(LeaderboardErrors error)
        {
            Error = error;
        }

        public ChannelboardViewModel(SocketGuild guild, SocketGuildUser user, List<ChannelStatItem> items) : this(LeaderboardErrors.Success)
        {
            Guild = guild;

            if (user != null)
                User = SimpleUserInfo.Create(user);

            Items = items;
        }
    }
}
