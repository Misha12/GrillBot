using Discord.WebSocket;
using System;
using System.Collections.Generic;

namespace Grillbot.Models.TempUnverify.Admin
{
    public class AuditItemSetOperation : AuditItemUpdateOperation
    {
        public DateTime StartAt { get; set; }
        public List<SocketRole> Roles { get; set; }
        public List<SocketGuildChannel> OverridedChannels { get; set; }
        public string Reason { get; set; }
    }
}
