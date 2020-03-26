using System.Threading.Tasks;

namespace Grillbot.Extensions
{
    public static class TaskExtensions
    {
        public static void RunSync(this Task task) => task.GetAwaiter().GetResult();
        public static TResult RunSync<TResult>(this Task<TResult> task) => task.GetAwaiter().GetResult();
    }
}
