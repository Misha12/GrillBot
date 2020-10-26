using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;

namespace Grillbot.Models.Unverify
{
    public class UnverifyAuditViewModel
    {
        public List<SocketGuild> Guilds { get; set; }
        public UnverifyAuditFilterFormData FormData { get; set; }
        public List<UnverifyLogItem> LogItems { get; set; }

        public PaginationInfo PaginationInfo { get; set; }

        public UnverifyAuditViewModel(DiscordSocketClient client, List<UnverifyLogItem> logItems, UnverifyAuditFilterFormData formData, PaginationInfo paginationInfo)
        {
            Guilds = client.Guilds.ToList();
            FormData = formData ?? new UnverifyAuditFilterFormData();
            LogItems = logItems;
            PaginationInfo = paginationInfo;
        }
    }
}
