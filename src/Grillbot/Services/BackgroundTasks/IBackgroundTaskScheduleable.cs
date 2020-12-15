using Grillbot.Models;
using System;
using System.Collections.Generic;

namespace Grillbot.Services.BackgroundTasks
{
    public interface IBackgroundTaskScheduleable
    {
        bool CanScheduleTask(DateTime lastScheduleAt);
        List<BackgroundTask> GetBackgroundTasks();
    }
}
