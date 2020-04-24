using Discord;
using Discord.WebSocket;
using System.Collections.Generic;

namespace Grillbot.Models.TempUnverify.Admin
{
    public class AuditItemRemoveOperation
    {
        public List<SocketRole> Roles { get; set; }
        public List<SocketGuildChannel> OverridedChannels { get; set; }
    }
}
