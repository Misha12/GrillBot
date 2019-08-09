using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Grillbot.Modules;
using Grillbot.Services;
using Grillbot.Services.Statistics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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
                .AddSingleton<DiscordService>()
                .AddSingleton<AutoReplyModule>()
                .AddSingleton<EmoteChain>();
        }

        public void Configure(IApplicationBuilder app, IApplicationLifetime lifetime)
        {
            var serviceProvider = app.ApplicationServices;
            ServiceProvider = serviceProvider;

            app
                .UseCors(o => o.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin())
                .UseMvc()
                .UseWelcomePage();

            serviceProvider.GetRequiredService<LoggingService>();
            serviceProvider.GetRequiredService<MessageHandler>();

            serviceProvider.GetRequiredService<Statistics>().Init().Wait();
            serviceProvider.GetRequiredService<DiscordService>().StartAsync().Wait();
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
