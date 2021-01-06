using Discord;
using Discord.WebSocket;
using Grillbot.Services.Audit;
using Grillbot.Services.BackgroundTasks;

namespace Grillbot.Extensions.Infrastructure
{
    public static class BackgroundTaskQueueExtensions
    {
        public static void ScheduleDownloadAuditLog(this BackgroundTaskQueue queue, ActionType type, SocketGuild guild)
        {
            var data = new DownloadAuditLogBackgroundTask(guild, type);
            queue.Add(data);
        }
    }
}
