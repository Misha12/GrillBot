using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Grillbot.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Grillbot.Services.Initiable;
using Grillbot.Services.Permissions.Api;
using System;
using System.Collections.Generic;
using Grillbot.Services.BackgroundTasks;
using Microsoft.AspNetCore.StaticFiles;
using Grillbot.Database;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Grillbot.Models.Config.AppSettings;
using Grillbot.FileSystem;

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
            var connectionString = Configuration.GetConnectionString("Default");
            var basePath = Configuration.GetValue<string>("FilesBasePath");

            services
                .AddFileSystem(builder => builder.Use(basePath))
                .AddDatabase(connectionString)
                .AddWebAuthentication()
                .AddHandlers()
                .AddLoggers()
                .AddMemoryCache()
                .AddCors()
                .AddMessageCache()
                .AddHttpClient()
                .AddScoped<FileExtensionContentTypeProvider>()
                .AddControllersWithViews();

            var pages = services.AddRazorPages();

#if DEBUG
            pages.AddRazorRuntimeCompilation();
#endif

            const GatewayIntents intents = GatewayIntents.Guilds | GatewayIntents.GuildMembers | GatewayIntents.GuildBans | GatewayIntents.GuildEmojis | GatewayIntents.GuildIntegrations | GatewayIntents.GuildInvites |
                GatewayIntents.GuildVoiceStates | GatewayIntents.GuildPresences | GatewayIntents.GuildMessages | GatewayIntents.GuildMessageReactions | GatewayIntents.GuildMessageTyping | GatewayIntents.DirectMessages |
                GatewayIntents.DirectMessageReactions | GatewayIntents.DirectMessageTyping;
            var config = new DiscordSocketConfig()
            {
                LogLevel = LogSeverity.Verbose,
                MessageCacheSize = 50000,
                GatewayIntents = intents
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
                .AddSingleton<InitService>()
                .AddSingleton(new BotState())
                .AddBotFeatures()
                .AddMath()
                .AddEmoteChain()
                .AddStatistics()
                .AddPaginationServices()
                .AddDuckServices()
                .AddHelpServices()
                .AddBackgroundTasks()
                .AddHostedService<GrillBotService>();

            services
                .Configure<WebAdminConfiguration>(Configuration.GetSection("WebAdmin"));
        }

        public void Configure(IApplicationBuilder app, GrillBotContext context)
        {
            if (context.Database.GetPendingMigrations().Any())
                context.Database.Migrate();

            var serviceProvider = app.ApplicationServices;

            app
                .UseCors(o => o.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin())
                .UseRouting()
                .UseMiddleware<DiscordAuthorizeMiddleware>()
                .UseAuthentication()
                .UseAuthorization()
                .UseStaticFiles()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                    endpoints.MapRazorPages();
                    endpoints.MapControllerRoute(name: "default", pattern: "{controller=Unverify}/{action=Index}/{id?}");
                });

            serviceProvider.GetRequiredService<InitService>().Init();
        }
    }
}
