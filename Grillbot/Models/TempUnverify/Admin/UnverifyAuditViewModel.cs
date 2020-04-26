using Discord.WebSocket;
using Grillbot.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Grillbot.Models.TempUnverify.Admin
{
    public class UnverifyAuditViewModel
    {
        public List<SocketGuildUser> Users { get; set; }
        public List<SocketGuild> Guilds { get; set; }
        public UnverifyAuditFilterRequest Request { get; set; }
        public List<UnverifyAuditItem> LogItems { get; set; }

        public UnverifyAuditViewModel(DiscordSocketClient client, List<UnverifyAuditItem> logItems, UnverifyAuditFilterRequest request)
        {
            Users = client.Guilds.SelectMany(o => o.Users.ToList()).DistinctBy(o => o.Id).ToList();
            Guilds = client.Guilds.ToList();
            Request = request ?? new UnverifyAuditFilterRequest();
            LogItems = logItems;
        }
    }
}
