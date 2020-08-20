using Discord.WebSocket;
using System.Collections.Generic;

namespace Grillbot.Models.Unverify
{
    public class UnverifyRemoveOperation
    {
        public List<SocketRole> ReturnedRoles { get; set; }
        public List<SocketGuildChannel> ReturnedChannels { get; set; }
    }
}
