using Discord.WebSocket;
using Grillbot.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grillbot.Models.Math
{
    public class MathAuditLogViewModel
    {
        public List<MathAuditItem> Items { get; set; }
        public MathAuditLogFilter Filter { get; set; }
        public List<SocketGuild> Guilds { get; set; }
        public List<SocketTextChannel> Channels { get; set; }
        public Dictionary<ulong, string> Users { get; set; }

        public MathAuditLogViewModel(DiscordSocketClient client, List<MathAuditItem> items, MathAuditLogFilter filter,
            Dictionary<ulong, string> users)
        {
            Filter = filter ?? new MathAuditLogFilter();
            Items = items ?? new List<MathAuditItem>();
            Guilds = client.Guilds.ToList();
            Channels = client.Guilds.SelectMany(o => o.TextChannels.ToList()).ToList();
            Users = users;
        }
    }
}
