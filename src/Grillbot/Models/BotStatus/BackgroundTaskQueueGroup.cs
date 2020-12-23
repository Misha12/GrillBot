namespace Grillbot.Models.BotStatus
{
    public class BackgroundTaskQueueGroup
    {
        public string ServiceName { get; set; }
        public string TaskName { get; set; }
        public int CanProcessCount { get; set; }
        public int CantProcessCount { get; set; }

        public BackgroundTaskQueueGroup(BackgroundTask task)
        {
            ServiceName = task.TaskType.Name;
            TaskName = task.GetType().Name.Replace("BackgroundTask", "");
        }
    }
}
