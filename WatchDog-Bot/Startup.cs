using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Threading.Tasks;
using WatchDog_Bot.Exceptions;
using WatchDog_Bot.Modules;
using WatchDog_Bot.Repository;
using WatchDog_Bot.Services.Statistics;

namespace WatchDog_Bot
{
    public class Startup
    {
        private IConfigurationRoot Configuration { get; }

        public Startup(string[] args)
        {
            if (args.Length < 1 || !File.Exists(args[0]))
                throw new ConfigException("Cannot found config. Please specify in first command line argument.");

            var builder = new ConfigurationBuilder()
                .AddJsonFile(args[0], optional: false);

            Configuration = builder.Build();
        }

        public async Task RunAsync()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);

            var provider = services.BuildServiceProvider();

            provider.GetRequiredService<LoggingService>();
            provider.GetRequiredService<CommandHandler>();

            Console.CancelKeyPress += (s, e) => Environment.Exit(0);

            await provider.GetRequiredService<Statistics>().Init();
            await provider.GetRequiredService<StartupService>().StartAsync();
            await Task.Delay(-1);
        }

        private void ConfigureServices(IServiceCollection services)
        {
            var config = new DiscordSocketConfig()
            {
                LogLevel = LogSeverity.Verbose,
                MessageCacheSize = 10000000,
                ExclusiveBulkDelete = true
            };

            var commandsConfig = new CommandServiceConfig()
            {
                LogLevel = LogSeverity.Verbose,
                DefaultRunMode = RunMode.Async,
                CaseSensitiveCommands = true
            };

            services
                .AddSingleton(new CommandService(commandsConfig))
                .AddSingleton(new DiscordSocketClient(config))
                .AddSingleton<Statistics>()
                .AddSingleton<CommandHandler>()
                .AddSingleton<LoggingService>()
                .AddSingleton<StartupService>()
                .AddSingleton<AutoReplyModule>()
                .AddSingleton(Configuration);
        }
    }
}
