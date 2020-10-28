using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Grillbot.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Grillbot.Services.Initiable;
using Grillbot.Models.Config.AppSettings;
using Grillbot.Services.Permissions.Api;

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

            var connectionString = Configuration.GetConnectionString("Default");

            services
                .AddDatabase(connectionString)
                .AddWebAuthentication()
                .AddHandlers()
                .AddLoggers()
                .AddMemoryCache()
                .AddCors()
                .AddMessageCache()
                .AddHttpClient();

            services
                .AddControllersWithViews();

            var pages = services.AddRazorPages();

#if DEBUG
            pages.AddRazorRuntimeCompilation();
#endif

            var intents = GatewayIntents.Guilds | GatewayIntents.GuildMembers | GatewayIntents.GuildBans | GatewayIntents.GuildEmojis | GatewayIntents.GuildIntegrations | GatewayIntents.GuildInvites |
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
                .AddSingleton(new BotState());

            services
                .AddAutoReply()
                .AddMath()
                .AddEmoteChain()
                .AddWebAdminServices()
                .AddStatistics()
                .AddPermissionsServices()
                .AddPaginationServices()
                .AddMemeImages()
                .AddChannelboard()
                .AddUnverify()
                .AddTeamSearch()
                .AddUserManagement()
                .AddDiscordAdminServices()
                .AddDuckServices()
                .AddHelpServices();

            services
                .AddHostedService<GrillBotService>();
        }

        public void Configure(IApplicationBuilder app)
        {
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
