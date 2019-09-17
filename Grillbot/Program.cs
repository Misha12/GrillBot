using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace Grillbot
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<AppStartup>()
                .ConfigureLogging(o => o.SetMinimumLevel(LogLevel.Warning))
                .Build()
                .Run();
        }
    }

}
