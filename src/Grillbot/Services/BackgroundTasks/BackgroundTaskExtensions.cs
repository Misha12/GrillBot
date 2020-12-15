using Microsoft.Extensions.DependencyInjection;

namespace Grillbot.Services.BackgroundTasks
{
    public static class BackgroundTaskExtensions
    {
        public static IServiceCollection AddBackgroundTasks(this IServiceCollection services)
        {
            services
                .AddSingleton<BackgroundTaskQueue>()
                .AddHostedService<BackgroundTaskService>()
                .AddHostedService<BackgroundTaskScheduler>();

            return services;
        }
    }
}
