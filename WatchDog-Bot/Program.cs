using System.Threading.Tasks;

namespace WatchDog_Bot
{
    public static class Program
    {
        public static Task Main(string[] args)
        {
            var startup = new Startup(args);
            return startup.RunAsync();
        }
    }
}
