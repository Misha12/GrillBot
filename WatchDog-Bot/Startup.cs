using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace WatchDog_Bot
{
    public class Startup
    {
        private IConfiguration Configuration { get; }

        public static async Task RunAsync(string[] args)
        {
            var startup = new Startup(args);
            await startup.RunAsync();
        }

        public Startup(string[] args)
        {
            //TODO Config Parser
        }

        public async Task RunAsync()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);

            var provider = services.BuildServiceProvider();
            
            //TODO Logging
            //TODO Command handler
            //TODO Start Bot

            await Task.Delay(-1);
        }

        private void ConfigureServices(IServiceCollection services)
        {
            var config = new DiscordSocketConfig() { LogLevel = Discord.LogSeverity.Verbose, MessageCacheSize = 1000 };
            var client = new DiscordSocketClient(config);

            services.AddSingleton(client);

            // TODO Register logging
            // TODO register command handler
            // todo register startupservice

            services.AddSingleton(Configuration);
        }
    }
}
