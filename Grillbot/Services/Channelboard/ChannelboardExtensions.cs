using Grillbot.Database.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace Grillbot.Services.Channelboard
{
    public static class ChannelboardExtensions
    {
        public static IServiceCollection AddChannelboard(this IServiceCollection services)
        {
            services
                .AddTransient<ChannelStatsRepository>()
                .AddTransient<ChannelboardWeb>()
                .AddSingleton<ChannelStats>();

            return services;
        }
    }
}
