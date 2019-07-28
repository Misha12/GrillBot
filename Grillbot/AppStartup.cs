using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Grillbot;
using Grillbot.Modules;
#pragma warning disable CS0234 // The type or namespace name 'Services' does not exist in the namespace 'Grillbot' (are you missing an assembly reference?)
using Grillbot.Services;
#pragma warning restore CS0234 // The type or namespace name 'Services' does not exist in the namespace 'Grillbot' (are you missing an assembly reference?)
#pragma warning disable CS0234 // The type or namespace name 'Services' does not exist in the namespace 'Grillbot' (are you missing an assembly reference?)
using Grillbot.Services.Statistics;
#pragma warning restore CS0234 // The type or namespace name 'Services' does not exist in the namespace 'Grillbot' (are you missing an assembly reference?)
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Grillbot
{
    public class AppStartup
    {
        public IConfiguration Configuration { get; }

        public AppStartup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            ConfigureDiscord(services);
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

            app
                .UseMvc();

            serviceProvider.GetRequiredService<LoggingService>();
            serviceProvider.GetRequiredService<MessageHandler>();

            serviceProvider.GetRequiredService<Statistics>().Init().Wait();
            serviceProvider.GetRequiredService<DiscordService>().StartAsync().Wait();
        }
    }
}
