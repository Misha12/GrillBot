using Discord.WebSocket;
using Grillbot.Database.Enums;
using System;
using System.Collections.Generic;

namespace Grillbot.Models.Unverify
{
    public class UnverifyAuditFilter
    {
        public SocketGuild Guild { get; set; }
        public List<SocketGuildUser> FromUsers { get; set; }
        public List<SocketGuildUser> ToUsers { get; set; }
        public UnverifyLogOperation? Operation { get; set; }
        public DateTime? DateTimeFrom { get; set; }
        public DateTime? DateTimeTo { get; set; }
        public bool OrderAsc { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }
    }
}
