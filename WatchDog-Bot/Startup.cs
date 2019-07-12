using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Threading.Tasks;
using WatchDog_Bot.Exceptions;

namespace WatchDog_Bot
{
    public class Startup
    {
        private IConfigurationRoot Configuration { get; }

        public static async Task RunAsync(string[] args)
        {
            var startup = new Startup(args);
            await startup.RunAsync();
        }

        public Startup(string[] args)
        {
            if (args.Length < 1 || !File.Exists(args[0]))
                throw new ConfigException("Cannot found config. Please specify in first command line argument.");

            var builder = new ConfigurationBuilder()
                .AddJsonFile(args[0], optional: false, reloadOnChange: true);

            Configuration = builder.Build();
        }

        public async Task RunAsync()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);

            var provider = services.BuildServiceProvider();

            provider.GetRequiredService<LoggingService>();
            //TODO Command handler
            //TODO Start Bot

            await provider.GetRequiredService<StartupService>().StartAsync();
            await Task.Delay(-1);
        }

        private void ConfigureServices(IServiceCollection services)
        {
            var config = new DiscordSocketConfig() { LogLevel = LogSeverity.Verbose, MessageCacheSize = 1000 };
            var client = new DiscordSocketClient(config);

            var commandsConfig = new CommandServiceConfig() { LogLevel = LogSeverity.Verbose, DefaultRunMode = RunMode.Async };
            var commands = new CommandService(commandsConfig);

            services
                .AddSingleton(commands)
                .AddSingleton(client);

            // TODO register command handler
            // todo register startupservice

            services
                .AddSingleton<LoggingService>()
                .AddSingleton<StartupService>()
                .AddSingleton(Configuration);
        }
    }
}
