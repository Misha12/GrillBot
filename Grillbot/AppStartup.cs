using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Grillbot.Handlers;
using Grillbot.Modules;
using Grillbot.Services;
using Grillbot.Services.Config;
using Grillbot.Services.Statistics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;

namespace Grillbot
{
    public class AppStartup
    {
        public IConfiguration Configuration { get; }
        private byte[] ActualConfigHash { get; set; }
        private IServiceProvider ServiceProvider { get; set; }

        public AppStartup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddCors()
                .AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            ConfigureDiscord(services);

            ActualConfigHash = GetConfigHash();
            ChangeToken.OnChange(() => Configuration.GetReloadToken(), OnConfigChange);
        }

        private void ConfigureDiscord(IServiceCollection services)
        {
            var config = new DiscordSocketConfig()
            {
                LogLevel = LogSeverity.Verbose,
                MessageCacheSize = 100000
            };

            var commandsConfig = new CommandServiceConfig()
            {
                LogLevel = LogSeverity.Verbose,
                DefaultRunMode = RunMode.Async,
                CaseSensitiveCommands = true
            };

            // Handlers
            services
                .AddSingleton<MessageReceivedHandler>()
                .AddSingleton<UserJoinedHandler>()
                .AddSingleton<MessageDeletedHandler>();

            services
                .AddSingleton(new CommandService(commandsConfig))
                .AddSingleton(new DiscordSocketClient(config))
                .AddSingleton<Statistics>()
                .AddSingleton<BotLoggingService>()
                .AddSingleton<GrillBotService>()
                .AddSingleton<AutoReplyService>()
                .AddSingleton<EmoteChain>()
                .AddSingleton<LoggerCache>()
                .AddTransient<MathCalculator>();

            services.AddHostedService<GrillBotService>();
        }

        public void Configure(IApplicationBuilder app)
        {
            var serviceProvider = app.ApplicationServices;
            ServiceProvider = serviceProvider;

            app
                .UseCors(o => o.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin())
                .UseMvc()
                .UseWelcomePage();

            var toInit = new[]
            {
                typeof(BotLoggingService),
                typeof(MessageReceivedHandler),
                typeof(UserJoinedHandler),
                typeof(MessageDeletedHandler)
            };

            InitServices(ServiceProvider, toInit);
            serviceProvider.GetRequiredService<Statistics>().Init();
        }

        private void InitServices(IServiceProvider provider, Type[] services)
        {
            foreach(var service in services)
            {
                provider.GetRequiredService(service);
            }
        }

        private void OnConfigChange()
        {
            var newHash = GetConfigHash();

            if(!ActualConfigHash.SequenceEqual(newHash))
            {
                var changeableTypes = Assembly.GetExecutingAssembly()
                    .GetTypes().Where(o => o.GetInterface(typeof(IConfigChangeable).FullName) != null);

                foreach(var type in changeableTypes)
                {
                    var service = (IConfigChangeable)ServiceProvider.GetService(type);
                    service?.ConfigChanged(Configuration);
                }

                ActualConfigHash = newHash;
                Console.WriteLine($"{DateTime.Now.ToLongTimeString()} BOT\tUpdated config.");
            }
        }

        private byte[] GetConfigHash()
        {
            if (File.Exists("appsettings.json"))
            {
                using (var fs = File.OpenRead("appsettings.json"))
                {
                    using(var sha1 = SHA1.Create())
                    {
                        return sha1.ComputeHash(fs);
                    }
                }
            }
            else
            {
                return new byte[20];
            }
        }
    }
}
