using System;

namespace Grillbot.Models
{
    public abstract class BackgroundTask
    {
        public abstract Type TaskType { get; }
        public virtual bool CanProcess() => true;
    }

    public abstract class BackgroundTask<TService> : BackgroundTask
    {
        public override Type TaskType { get; } = typeof(TService);
    }
}
