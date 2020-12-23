using Discord;
using Discord.WebSocket;
using Grillbot.Models;

namespace Grillbot.Services.Audit
{
    public class DownloadAuditLogBackgroundTask : BackgroundTask<AuditService>
    {
        public ulong GuildId { get; set; }
        public ActionType ActionType { get; set; }

        public DownloadAuditLogBackgroundTask(SocketGuild guild, ActionType type)
        {
            GuildId = guild.Id;
            ActionType = type;
        }
    }
}
