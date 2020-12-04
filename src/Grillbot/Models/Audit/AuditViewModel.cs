using Discord.WebSocket;
using System.Collections.Generic;

namespace Grillbot.Models.Audit
{
    public class AuditViewModel
    {
        public List<AuditItem> Items { get; set; }
        public LogsFilter Filter { get; set; }
        public PaginationInfo PaginationInfo { get; set; }
        public List<SocketGuild> Guilds { get; set; }

        public AuditViewModel(List<AuditItem> items, LogsFilter filter, PaginationInfo paginationInfo, List<SocketGuild> guilds)
        {
            Items = items;
            Filter = filter;
            PaginationInfo = paginationInfo;
            Guilds = guilds;
        }
    }
}
