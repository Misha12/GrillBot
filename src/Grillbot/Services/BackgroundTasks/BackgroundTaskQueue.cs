using Grillbot.Models;
using Grillbot.Models.BotStatus;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Grillbot.Services.BackgroundTasks
{
    public class BackgroundTaskQueue
    {
        private ConcurrentDictionary<Guid, BackgroundTask> Tasks { get; }

        public BackgroundTaskQueue()
        {
            Tasks = new ConcurrentDictionary<Guid, BackgroundTask>();
        }

        public BackgroundTask PopAvailable()
        {
            var task = Tasks
                .FirstOrDefault(o => o.Value.CanProcess());

            if (task.Value == null)
                return null;

            Tasks.TryRemove(task);
            return task.Value;
        }

        public void Add(BackgroundTask task)
        {
            if (task == null)
                return;

            Tasks.TryAdd(Guid.NewGuid(), task);
        }

        public void TryRemove<TBackgroundTaskType>(Func<TBackgroundTaskType, bool> selector) where TBackgroundTaskType : BackgroundTask
        {
            var tasks = Tasks
                .Where(o => o.Value is TBackgroundTaskType task && selector(task))
                .ToList();

            if (tasks.Count == 0)
                return;

            foreach(var task in tasks)
            {
                Tasks.TryRemove(task);
            }
        }

        public TBackgroundTask Get<TBackgroundTask>(Func<TBackgroundTask, bool> selector) where TBackgroundTask : BackgroundTask
        {
            return (TBackgroundTask)Tasks
                .FirstOrDefault(o => o.Value is TBackgroundTask task && selector(task)).Value;
        }

        public bool Exists<TBackgroundTask>(Func<TBackgroundTask, bool> selector) where TBackgroundTask : BackgroundTask
        {
            return Tasks.Any(o => o.Value is TBackgroundTask task && selector(task));
        }

        public List<BackgroundTaskQueueGroup> GetStatus()
        {
            var groups = Tasks
                .Select(o => o.Value)
                .GroupBy(o => $"{o.GetType().Name}/{o.TaskType.Name}")
                .Select(o => new BackgroundTaskQueueGroup(o.First())
                {
                    CanProcessCount = o.Count(o => o.CanProcess()),
                    CantProcessCount = o.Count(o => !o.CanProcess())
                });

            return groups.ToList();
        }
    }
}
