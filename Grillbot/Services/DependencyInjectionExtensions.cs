using Grillbot.Database;
using Grillbot.Database.Repository;
using Grillbot.Handlers;
using Grillbot.Modules.AutoReply;
using Grillbot.Services.AdminServices;
using Grillbot.Services.Channelboard;
using Grillbot.Services.Duck;
using Grillbot.Services.Math;
using Grillbot.Services.MemeImages;
using Grillbot.Services.MessageCache;
using Grillbot.Services.Permissions;
using Grillbot.Services.Reminder;
using Grillbot.Services.Statistics;
using Grillbot.Services.TeamSearch;
using Grillbot.Services.TempUnverify;
using Grillbot.Services.Unverify;
using Grillbot.Services.UserManagement;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Grillbot.Services
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddAutoReply(this IServiceCollection services)
        {
            services
                .AddSingleton<AutoReplyService>();

            return services;
        }

        public static IServiceCollection AddDatabase(this IServiceCollection services, string connectionString)
        {
            services
                .AddDbContext<GrillBotContext>(options => options.UseSqlServer(connectionString), ServiceLifetime.Transient, ServiceLifetime.Transient)
                .AddTransient<AutoReplyRepository>()
                .AddTransient<BotDbRepository>()
                .AddTransient<ConfigRepository>()
                .AddTransient<EmoteStatsRepository>()
                .AddTransient<ChannelStatsRepository>()
                .AddTransient<TeamSearchRepository>()
                .AddTransient<TempUnverifyRepository>()
                .AddTransient<UsersRepository>()
                .AddTransient<MathRepository>()
                .AddTransient<UserStatisticsRepository>()
                .AddTransient<GlobalConfigRepository>()
                .AddTransient<ReminderRepository>()
                .AddTransient<InviteRepository>()
                .AddTransient<UnverifyRepository>();

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
                .AddSingleton<CommandExecutedHandler>();

            return services;
        }

        public static IServiceCollection AddMath(this IServiceCollection services)
        {
            services
                .AddSingleton<MathService>()
                .AddTransient<MathAuditService>();

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
                    .AddConsole(consoleConfig =>
                    {
                        consoleConfig.TimestampFormat = "[dd. MM. yyyy HH:mm:ss]\t";
                        consoleConfig.IncludeScopes = true;
                    });
            });

            services
                .AddSingleton<BotLoggingService>()
                .AddSingleton<Logger.Logger>();

            return services;
        }

        public static IServiceCollection AddEmoteChain(this IServiceCollection services)
        {
            services
                .AddSingleton<EmoteChain>();

            return services;
        }

        public static IServiceCollection AddCReference(this IServiceCollection services)
        {
            services
                .AddSingleton<CReferenceService>();

            return services;
        }

        public static IServiceCollection AddWebAdminServices(this IServiceCollection services)
        {
            services
                .AddSingleton<BotStatusService>();

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
                .AddSingleton<EmoteStats>()
                .AddSingleton<InternalStatistics>();

            return services;
        }

        public static IServiceCollection AddPermissionsServices(this IServiceCollection services)
        {
            services
                .AddTransient<PermissionsManager>();

            return services;
        }

        public static IServiceCollection AddPaginationServices(this IServiceCollection services)
        {
            services
                .AddSingleton<PaginationService>();

            return services;
        }

        public static IServiceCollection AddMemeImages(this IServiceCollection services)
        {
            services
                .AddTransient<MemeImagesService>();

            return services;
        }

        public static IServiceCollection AddUnverify(this IServiceCollection services)
        {
            services
                .AddTransient<TempUnverifyLogService>()
                .AddSingleton<TempUnverifyService>()
                .AddTransient<TempUnverifyRoleManager>();

            services
                .AddTransient<UnverifyService>()
                .AddTransient<UnverifyChecker>()
                .AddTransient<UnverifyLogger>()
                .AddTransient<UnverifyMessageGenerator>()
                .AddTransient<UnverifyProfileGenerator>()
                .AddTransient<UnverifyReasonParser>()
                .AddTransient<UnverifyTimeParser>();

            return services;
        }

        public static IServiceCollection AddTeamSearch(this IServiceCollection services)
        {
            services
                .AddTransient<TeamSearchService>();

            return services;
        }

        public static IServiceCollection AddChannelboard(this IServiceCollection services)
        {
            services
                .AddTransient<ChannelboardWeb>()
                .AddTransient<ChannelStats>();

            return services;
        }

        public static IServiceCollection AddUserManagement(this IServiceCollection services)
        {
            services
                .AddSingleton<UserService>()
                .AddTransient<PointsService>()
                .AddTransient<BirthdayService>()
                .AddTransient<ReminderService>()
                .AddSingleton<ReminderTaskService>();

            return services;
        }

        public static IServiceCollection AddDiscordAdminServices(this IServiceCollection services)
        {
            services
                .AddTransient<PinManagement>()
                .AddTransient<InviteTrackerService>();

            return services;
        }

        public static IServiceCollection AddDuckServices(this IServiceCollection services)
        {
            services
                .AddTransient<DuckDataLoader>()
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
