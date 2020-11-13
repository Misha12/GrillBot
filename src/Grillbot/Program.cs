using Grillbot.Services.Config;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Grillbot
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<AppStartup>())
                .ConfigureAppConfiguration(builder =>
                {
                    var connectionString = builder.Build().GetConnectionString("Default");
                    builder.Add(new ConfigSource(connectionString, "global"));
                });
        }
    }
}
