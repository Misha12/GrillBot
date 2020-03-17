using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;

namespace Grillbot
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(config =>
                {
                    config
                        .ConfigureLogging(logging => logging.SetMinimumLevel(LogLevel.Information))
                        .UseStartup<AppStartup>();
                })
                .Build().Run();
        }
    }
}
