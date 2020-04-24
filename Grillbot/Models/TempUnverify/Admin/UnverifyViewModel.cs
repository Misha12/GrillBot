using Discord;
using Discord.WebSocket;
using System.Collections.Generic;

namespace Grillbot.Models.TempUnverify.Admin
{
    public class UnverifyViewModel
    {
        public List<SocketGuildUser> Users { get; set; }
        public List<SocketGuild> Guilds { get; set; }
        public UnverifyAuditFilterRequest Request { get; set; }
        public List<UnverifyAuditItem> LogItems { get; set; }
        public List<CurrentUnverifiedUser> CurrentUnverified { get; set; }
    }
}
