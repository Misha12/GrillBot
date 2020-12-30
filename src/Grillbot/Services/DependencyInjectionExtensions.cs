using Grillbot.Database;
using Grillbot.Handlers;
using Grillbot.Modules.AutoReply;
using Grillbot.Services.AdminServices;
using Grillbot.Services.Audit;
using Grillbot.Services.Channelboard;
using Grillbot.Services.Config;
using Grillbot.Services.Duck;
using Grillbot.Services.ErrorHandling;
using Grillbot.Services.InviteTracker;
using Grillbot.Services.Math;
using Grillbot.Services.MemeImages;
using Grillbot.Services.MessageCache;
using Grillbot.Services.Permissions;
using Grillbot.Services.Reminder;
using Grillbot.Services.Statistics;
using Grillbot.Services.Statistics.ApiStats;
using Grillbot.Services.TeamSearch;
using Grillbot.Services.Unverify;
using Grillbot.Services.Unverify.WebAdmin;
using Grillbot.Services.UserManagement;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Grillbot.Services
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddBotFeatures(this IServiceCollection services)
        {
            return services
                .AddScoped<EmoteStats>()
                .AddScoped<AutoReplyService>()
                .AddScoped<BotStatusService>()
                .AddScoped<ChannelboardWeb>()
                .AddScoped<DuckDataLoader>()
                .AddScoped<MemeImagesService>()
                .AddScoped<PermissionsManager>()
                .AddScoped<PinManagement>()
                .AddSingleton<ConfigurationService>()
                .AddScoped<ChannelStats>()
                .AddScoped<InviteTrackerService>()
                .AddScoped<ReminderService>()
                .AddScoped<TeamSearchService>()
                .AddScoped<UnverifyChecker>()
                .AddScoped<UnverifyService>()
                .AddScoped<UnverifyLogger>()
                .AddScoped<UnverifyProfileGenerator>()
                .AddScoped<UnverifyMessageGenerator>()
                .AddScoped<UnverifyReasonParser>()
                .AddScoped<UnverifyTimeParser>()
                .AddScoped<UnverifyModelConverter>()
                .AddScoped<WebAccessService>()
                .AddScoped<UserService>()
                .AddScoped<UserReactionsService>()
                .AddScoped<UserMessagesService>()
                .AddScoped<PointsService>()
                .AddScoped<PointsRenderService>()
                .AddScoped<UserSearchService>()
                .AddScoped<BirthdayService>()
                .AddScoped<AuditService>();
        }

        public static IServiceCollection AddDatabase(this IServiceCollection services, string connectionString)
        {
            services
                .AddDbContext<GrillBotContext>(options =>
                {
                    options.UseSqlServer(connectionString);
                    //options.EnableSensitiveDataLogging(); // In a case of emergency. Only in DEBUG.
                }, ServiceLifetime.Transient, ServiceLifetime.Transient)
                .AddTransient<IGrillBotRepository, GrillBotRepository>();

            return services;
        }

        public static IServiceCollection AddWebAuthentication(this IServiceCollection services)
        {
            services
                .AddScoped<WebAuthenticationService>()
                .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(opt =>
                {
                    opt.LoginPath = "/Login";
                    opt.LogoutPath = "/Logout";
                });

            return services;
        }

        public static IServiceCollection AddHandlers(this IServiceCollection services)
        {
            services
                .AddSingleton<GuildMemberUpdatedHandler>()
                .AddSingleton<MessageDeletedHandler>()
                .AddSingleton<MessageEditedHandler>()
                .AddSingleton<MessageReceivedHandler>()
                .AddSingleton<ReactionAddedHandler>()
                .AddSingleton<ReactionRemovedHandler>()
                .AddSingleton<UserJoinedHandler>()
                .AddSingleton<UserLeftHandler>()
                .AddSingleton<CommandExecutedHandler>()
                .AddSingleton<UnbanHandler>();

            return services;
        }

        public static IServiceCollection AddMath(this IServiceCollection services)
        {
            services
                .AddSingleton<MathService>();

            return services;
        }

        public static IServiceCollection AddLoggers(this IServiceCollection services)
        {
            services.AddLogging(opt =>
            {
                opt
                    .SetMinimumLevel(LogLevel.Information)
                    .AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning)
                    .AddFilter("Microsoft.EntityFrameworkCore.Infrastructure", LogLevel.Warning)
                    .AddFilter("Microsoft.AspNetCore.Hosting.Diagnostics", LogLevel.Warning)
                    .AddFilter("Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker", LogLevel.Warning)
                    .AddFilter("Microsoft.AspNetCore.Mvc.ViewFeatures.ViewResultExecutor", LogLevel.Warning)
                    .AddSimpleConsole(config =>
                    {
                        config.TimestampFormat = "dd. MM. yyyy HH:mm:ss\t";
                        config.IncludeScopes = true;
                    });
            });

            services
                .AddSingleton<BotLoggingService>()
                .AddTransient<LogEmbedCreator>()
                .AddSingleton<ApiStatistics>();

            return services;
        }

        public static IServiceCollection AddEmoteChain(this IServiceCollection services)
        {
            services
                .AddSingleton<EmoteChain>();

            return services;
        }

        public static IServiceCollection AddMessageCache(this IServiceCollection services)
        {
            services
                .AddSingleton<IMessageCache, MessageCache.MessageCache>();

            return services;
        }

        public static IServiceCollection AddStatistics(this IServiceCollection services)
        {
            services
                .AddSingleton<InternalStatistics>();

            return services;
        }

        public static IServiceCollection AddPaginationServices(this IServiceCollection services)
        {
            services
                .AddSingleton<PaginationService>();

            return services;
        }

        public static IServiceCollection AddDuckServices(this IServiceCollection services)
        {
            services
                .AddTransient<DuckEmbedRenderer>();

            return services;
        }

        public static IServiceCollection AddHelpServices(this IServiceCollection services)
        {
            services
                .AddTransient<HelpEmbedRenderer>();

            return services;
        }
    }
}
