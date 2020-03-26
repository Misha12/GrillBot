using Microsoft.Extensions.DependencyInjection;

namespace Grillbot.Services.TempUnverify
{
    public static class TempUnverifyExtensions
    {
        public static IServiceCollection AddTempUnverify(this IServiceCollection services)
        {
            services
                .AddSingleton<TempUnverifyService>()
                .AddTransient<TempUnverifyFactories>()
                .AddTransient<TempUnverifyChecker>()
                .AddTransient<TempUnverifyHelper>();

            return services;
        }
    }
}
