using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Grillbot.Handlers;
using Grillbot.Modules;
using Grillbot.Services;
using Grillbot.Services.Config;
using Grillbot.Services.Config.Models;
using Grillbot.Services.Logger;
using Grillbot.Services.MessageCache;
using Grillbot.Services.Statistics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Discord.Addons.Interactive;
using Grillbot.Middleware;
using Grillbot.Services.Math;
using Grillbot.Services.TempUnverify;
using Grillbot.Middleware.DiscordUserAuthorization;
using Grillbot.Services.Initiable;

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

            services.Configure<Configuration>(Configuration);
            services.AddTransient<OptionsWriter>();

            services
                .AddTransient<DcUserAuthorization>();
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

            services
                .AddSingleton<GuildMemberUpdatedHandler>()
                .AddSingleton<MessageDeletedHandler>()
                .AddSingleton<MessageEditedHandler>()
                .AddSingleton<MessageReceivedHandler>()
                .AddSingleton<ReactionAddedHandler>()
                .AddSingleton<ReactionRemovedHandler>()
                .AddSingleton<UserJoinedHandler>()
                .AddSingleton<UserLeftHandler>();

            services
                .AddSingleton(new CommandService(commandsConfig))
                .AddSingleton(new DiscordSocketClient(config))
                .AddSingleton<Statistics>()
                .AddSingleton<BotLoggingService>()
                .AddSingleton<GrillBotService>()
                .AddSingleton<AutoReplyService>()
                .AddSingleton<EmoteChain>()
                .AddSingleton<InteractiveService>()
                .AddSingleton<CReferenceService>()
                .AddSingleton<TempUnverifyService>()
                .AddSingleton<MathService>()
                .AddTransient<TeamSearchService>()
                .AddTransient<BotStatusService>()
                .AddSingleton<Logger>()
                .AddSingleton<IMessageCache, MessageCache>()
                .AddSingleton<CalledEventStats>()
                .AddSingleton<InitService>();

            services.AddHostedService<GrillBotService>();
        }

        public void Configure(IApplicationBuilder app)
        {
            var serviceProvider = app.ApplicationServices;
            ServiceProvider = serviceProvider;

            app
                .UseMiddleware<LogMiddleware>()
                .UseCors(o => o.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin())
                .UseMvc()
                .UseWelcomePage();

            serviceProvider.GetRequiredService<InitService>().Init();
            serviceProvider.GetRequiredService<Statistics>().Init();
        }

        private void OnConfigChange()
        {
            var newHash = GetConfigHash();

            if (!ActualConfigHash.SequenceEqual(newHash))
            {
                var changeableTypes = Assembly.GetExecutingAssembly()
                    .GetTypes().Where(o => o.GetInterface(typeof(IConfigChangeable).FullName) != null);

                var configurationInstance = Configuration.Get<Configuration>();
                var loggingService = ServiceProvider.GetRequiredService<BotLoggingService>();

                foreach (var type in changeableTypes)
                {
                    var service = (IConfigChangeable)ServiceProvider.GetService(type);
                    service?.ConfigChanged(configurationInstance);
                }

                var oldHash = ActualConfigHash;
                ActualConfigHash = newHash;

                loggingService.Write($"Updated config ({Convert.ToBase64String(oldHash)}) => ({Convert.ToBase64String(newHash)})");
                loggingService.SendConfigChangeInfo();
            }
        }

        private byte[] GetConfigHash()
        {
            if (File.Exists("appsettings.json"))
            {
                using (var fs = File.OpenRead("appsettings.json"))
                {
                    using (var sha1 = SHA1.Create())
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
