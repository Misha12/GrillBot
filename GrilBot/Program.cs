using System.Threading.Tasks;

namespace GrilBot
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
