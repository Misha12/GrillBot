using System.Threading.Tasks;

namespace Grillbot.Services.BackgroundTasks
{
    /// <summary>
    /// Interface for notify of processing background tasks.
    /// </summary>
    public interface IBackgroundTaskObserver
    {
        Task TriggerBackgroundTaskAsync(object data);
    }
}
