using Discord;
using Discord.WebSocket;
using Grillbot.Models;
using System;

namespace Grillbot.Services.Audit
{
    public class DownloadAuditLogBackgroundTask : BackgroundTask<AuditService>
    {
        public ulong GuildId { get; set; }
        public ActionType ActionType { get; set; }
        public DateTime RunAt { get; set; }

        public DownloadAuditLogBackgroundTask(SocketGuild guild, ActionType type, int wait = 0)
        {
            GuildId = guild.Id;
            ActionType = type;
            RunAt = DateTime.Now.AddSeconds(wait);
        }

        public override bool CanProcess()
        {
            return (RunAt - DateTime.Now).TotalSeconds <= 0.0F;
        }
    }
}
