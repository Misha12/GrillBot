using Grillbot.Database.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace Grillbot.Services.TempUnverify
{
    public static class TempUnverifyExtensions
    {
        public static IServiceCollection AddTempUnverify(this IServiceCollection services)
        {
            services
                .AddTransient<TempUnverifyLogService>()
                .AddTransient<TempUnverifyRepository>()
                .AddSingleton<TempUnverifyService>()
                .AddTransient<TempUnverifyFactories>()
                .AddTransient<TempUnverifyChecker>();

            return services;
        }
    }
}
