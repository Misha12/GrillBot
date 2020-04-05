using Microsoft.Extensions.DependencyInjection;

namespace Grillbot.Services.Statistics
{
    public static class StatisticsExtensions
    {
        public static IServiceCollection AddStatistics(this IServiceCollection services)
        {
            services
                .AddSingleton<InternalStatistics>();

            return services;
        }
    }
}
