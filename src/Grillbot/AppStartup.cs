using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Grillbot.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Grillbot.Services.Initiable;
using Grillbot.Services.Permissions.Api;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using Grillbot.Services.BackgroundTasks;

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
            var connectionString = Configuration.GetValue<string>("DB_CONN");

            services
                .AddDatabase(connectionString)
                .AddWebAuthentication()
                .AddHandlers()
                .AddLoggers()
                .AddMemoryCache()
                .AddCors()
                .AddMessageCache()
                .AddHttpClient();

            services.AddSwaggerGen(setup =>
            {
                var apiInfo = new OpenApiInfo()
                {
                    Contact = new OpenApiContact()
                    {
                        Name = "GrillBot",
                        Url = new Uri("https://github.com/Misha12/GrillBot")
                    },
                    License = new OpenApiLicense() { Name = "MIT" },
                    Title = "GrillBot API",
                    Version = "v1"
                };

                setup.SwaggerDoc("v1", apiInfo);

                setup.AddSecurityDefinition("GrillBot", new OpenApiSecurityScheme()
                {
                    Description = "GrillBot authorization token. BotAdmin user can generate this tokens for user (or bot) accounts.",
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Scheme = "GrillBot",
                    Type = SecuritySchemeType.ApiKey
                });

                setup.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme()
                        {
                            Reference = new OpenApiReference()
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "GrillBot"
                            }
                        }, new List<string>()
                    }
                });
            });

            services
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
                .AddSingleton(new BotState());

            services
                .AddBotFeatures()
                .AddMath()
                .AddEmoteChain()
                .AddStatistics()
                .AddPaginationServices()
                .AddDuckServices()
                .AddHelpServices()
                .AddBackgroundTasks();

            services
                .AddHostedService<GrillBotService>();
        }

        public void Configure(IApplicationBuilder app)
        {
            var serviceProvider = app.ApplicationServices;

            app
                .UseSwagger()
                .UseSwaggerUI(o => o.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1"))
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
