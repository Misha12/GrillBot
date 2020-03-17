using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Grillbot.Handlers;
using Grillbot.Services;
using Grillbot.Services.Config.Models;
using Grillbot.Services.Logger;
using Grillbot.Services.MessageCache;
using Grillbot.Services.Statistics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Discord.Addons.Interactive;
using Grillbot.Middleware;
using Grillbot.Services.Math;
using Grillbot.Services.TempUnverify;
using Grillbot.Middleware.DiscordUserAuthorization;
using Grillbot.Services.Initiable;
using Grillbot.Modules.AutoReply;
using Grillbot.Services.Permissions;
using Grillbot.Database;
using Microsoft.EntityFrameworkCore;
using Grillbot.Database.Repository;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Logging;

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
                        .EnableSensitiveDataLogging(false)
                        .UseSqlServer(Configuration.GetConnectionString("Default"));
                }, ServiceLifetime.Transient, ServiceLifetime.Transient);

            services
                .AddTransient<AutoReplyRepository>()
                .AddTransient<BirthdaysRepository>()
                .AddTransient<BotDbRepository>()
                .AddTransient<ConfigRepository>()
                .AddTransient<EmoteStatsRepository>()
                .AddTransient<ChannelStatsRepository>()
                .AddTransient<LogRepository>()
                .AddTransient<TeamSearchRepository>()
                .AddTransient<TempUnverifyRepository>();

            services
                .AddCors()
                .AddControllers();

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
                .AddTransient<BotStatusService>()
                .AddSingleton<Logger>()
                .AddSingleton<IMessageCache, MessageCache>()
                .AddSingleton<CalledEventStats>()
                .AddSingleton<InitService>()
                .AddSingleton<ChannelStats>()
                .AddSingleton<EmoteStats>()
                .AddSingleton<PermissionsManager>();

            services.AddHostedService<GrillBotService>();

            services
                .AddTransient<DcUserAuthorization>();
        }

        public void Configure(IApplicationBuilder app)
        {
            var serviceProvider = app.ApplicationServices;

            app
                .UseMiddleware<LogMiddleware>()
                .UseCors(o => o.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin())
                .UseRouting()
                .UseAuthorization()
                .UseEndpoints(endpoints => endpoints.MapControllers())
                .UseWelcomePage();

            serviceProvider.GetRequiredService<InitService>().Init();
        }
    }
}
