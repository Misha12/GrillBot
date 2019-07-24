using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using GrilBot.Exceptions;
using GrilBot.Modules;
using GrilBot.Services;
using GrilBot.Services.Statistics;

namespace GrilBot
{
    public class Startup
    {
        private IConfigurationRoot Configuration { get; }
        private IServiceProvider ServiceProvider { get; set; }

        private byte[] ConfigHash { get; set; }
        private string ConfigFilename { get; }

        public Startup(string[] args)
        {

            if (args.Length < 1 || !File.Exists(args[0]))
                throw new ConfigException("Cannot found config. Please specify in first command line argument.");

            var builder = new ConfigurationBuilder()
                .AddJsonFile(args[0], optional: false, reloadOnChange: true);

            Configuration = builder.Build();

            ConfigFilename = args[0];
            ConfigHash = GetConfigHash();

            ChangeToken.OnChange(() => Configuration.GetReloadToken(), ConfigChanged);
        }

        public async Task RunAsync()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);

            ServiceProvider = services.BuildServiceProvider();

            ServiceProvider.GetRequiredService<LoggingService>();
            ServiceProvider.GetRequiredService<MessageHandler>();

            Console.CancelKeyPress += (s, e) => Environment.Exit(0);

            await ServiceProvider.GetRequiredService<Statistics>().Init();
            await ServiceProvider.GetRequiredService<StartupService>().StartAsync();
            await Task.Delay(-1);
        }

        private void ConfigureServices(IServiceCollection services)
        {
            var config = new DiscordSocketConfig()
            {
                LogLevel = LogSeverity.Verbose,
                MessageCacheSize = 1000000
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
                .AddSingleton<MessageHandler>()
                .AddSingleton<LoggingService>()
                .AddSingleton<StartupService>()
                .AddSingleton<AutoReplyModule>()
                .AddSingleton(Configuration)
                .AddSingleton<EmoteChain>();
        }

        private void ConfigChanged()
        {
            var newHash = GetConfigHash();

            if (!ConfigHash.SequenceEqual(newHash))
            {
                var changeableTypes = Assembly.GetExecutingAssembly()
                    .GetTypes().Where(o => o.GetInterface(typeof(IConfigChangeable).FullName) != null);

                foreach (var type in changeableTypes)
                {
                    var service = (IConfigChangeable)ServiceProvider.GetService(type);
                    service?.ConfigChanged(Configuration);
                }

                ConfigHash = newHash;

                Console.WriteLine($"{DateTime.Now.ToLongTimeString()} BOT\tUpdated config.");
            }
        }

        private byte[] GetConfigHash()
        {
            if(File.Exists(ConfigFilename))
            {
                using (var fs = File.OpenRead(ConfigFilename))
                {
                    return SHA1.Create().ComputeHash(fs);
                }
            }
            else
            {
                return new byte[20];
            }
        }
    }
}
