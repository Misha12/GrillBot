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
        public bool IsSelfUnverify { get; set; }
        public List<string> Subjects { get; set; }

        public AuditItemSetOperation()
        {
            Roles = new List<SocketRole>();
            OverridedChannels = new List<SocketGuildChannel>();
            Subjects = new List<string>();
        }
    }
}
