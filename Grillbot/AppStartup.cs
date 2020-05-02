using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Grillbot.Handlers;
using Grillbot.Services;
using Grillbot.Services.Logger;
using Grillbot.Services.MessageCache;
using Grillbot.Services.Statistics;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Grillbot.Services.Math;
using Grillbot.Services.TempUnverify;
using Grillbot.Middleware.DiscordUserAuthorization;
using Grillbot.Services.Initiable;
using Grillbot.Modules.AutoReply;
using Grillbot.Services.Permissions;
using Grillbot.Database;
using Microsoft.EntityFrameworkCore;
using Grillbot.Database.Repository;
using Grillbot.Services.Channelboard;
using Microsoft.Extensions.Logging;
using Grillbot.Services.TeamSearch;
using Grillbot.Models.Config.AppSettings;
using Grillbot.Services.MemeImages;
using Grillbot.Services.WebAdmin;

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
            services.Configure<Configuration>(Configuration);

            services
                .AddDbContext<GrillBotContext>(options =>
                {
                    options
                        .UseSqlServer(Configuration.GetConnectionString("Default"));
                }, ServiceLifetime.Transient, ServiceLifetime.Transient);

            services
                .AddTransient<AutoReplyRepository>()
                .AddTransient<BirthdaysRepository>()
                .AddTransient<BotDbRepository>()
                .AddTransient<ConfigRepository>()
                .AddTransient<EmoteStatsRepository>();

            services.AddWebAuthentication();

            services
                .AddMemoryCache()
                .AddCors()
                .AddLogging(opt =>
                {
                    opt
                        .SetMinimumLevel(LogLevel.Information)
                        .AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning)
                        .AddFilter("Microsoft.EntityFrameworkCore.Infrastructure", LogLevel.Warning)
                        .AddConsole(consoleConfig =>
                        {
                            consoleConfig.TimestampFormat = "[dd. MM. yyyy HH:mm:ss]\t";
                            consoleConfig.IncludeScopes = true;
                        });
                });

            services.AddControllersWithViews();

            var pages = services.AddRazorPages();

#if DEBUG
            pages.AddRazorRuntimeCompilation();
#endif

            var config = new DiscordSocketConfig()
            {
                LogLevel = LogSeverity.Verbose,
                MessageCacheSize = 50000
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
                .AddSingleton<UserLeftHandler>()
                .AddSingleton<CommandExecutedHandler>();

            services
                .AddSingleton(new CommandService(commandsConfig))
                .AddSingleton(new DiscordSocketClient(config))
                .AddSingleton<BotLoggingService>()
                .AddSingleton<AutoReplyService>()
                .AddSingleton<EmoteChain>()
                .AddSingleton<CReferenceService>()
                .AddSingleton<MathService>()
                .AddTransient<BotStatusService>()
                .AddSingleton<Logger>()
                .AddSingleton<IMessageCache, MessageCache>()
                .AddSingleton<InitService>()
                .AddSingleton<EmoteStats>()
                .AddSingleton<PermissionsManager>()
                .AddSingleton<PaginationService>();

            services
                .AddMemeImages()
                .AddChannelboard()
                .AddTempUnverify()
                .AddTeamSearch()
                .AddStatistics()
                .AddWebAdmin();

            services.AddHostedService<GrillBotService>();

            services
                .AddTransient<DcUserAuthorization>();
        }

        public void Configure(IApplicationBuilder app)
        {
            var serviceProvider = app.ApplicationServices;

            app
                .UseCors(o => o.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin())
                .UseRouting()
                .UseAuthentication()
                .UseAuthorization()
                .UseStaticFiles()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                    endpoints.MapRazorPages();
                    endpoints.MapControllerRoute(name: "default", pattern: "{controller=Admin}/{action=Index}/{id?}");
                });

            serviceProvider.GetRequiredService<InitService>().Init();
        }
        }
}
