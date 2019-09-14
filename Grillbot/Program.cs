using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Grillbot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((_, config) => config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true))
                .UseStartup<AppStartup>()
                .ConfigureLogging(o => o.SetMinimumLevel(LogLevel.Warning));
        }
    }

}
