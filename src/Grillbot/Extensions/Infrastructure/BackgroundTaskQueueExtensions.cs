using Discord;
using Discord.WebSocket;
using Grillbot.Services.Audit;
using Grillbot.Services.BackgroundTasks;

namespace Grillbot.Extensions.Infrastructure
{
    public static class BackgroundTaskQueueExtensions
    {
        public static void ScheduleDownloadAuditLog(this BackgroundTaskQueue queue, ActionType type, SocketGuild guild, int wait = 0)
        {
            var data = new DownloadAuditLogBackgroundTask(guild, type, wait);
            queue.Add(data);
        }

        public static void ScheduleDownloadAuditLogIfNotExists(this BackgroundTaskQueue queue, ActionType type, SocketGuild guild, int wait = 0)
        {
            if (queue.Exists<DownloadAuditLogBackgroundTask>(o => o.ActionType == type && o.GuildId == guild.Id))
                return; // Now exists. Ignore.

            queue.ScheduleDownloadAuditLog(type, guild, wait);
        }
    }
}
