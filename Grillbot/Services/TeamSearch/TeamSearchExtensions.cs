using Grillbot.Database.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace Grillbot.Services.TeamSearch
{
    public static class TeamSearchExtensions
    {
        public static IServiceCollection AddTeamSearch(this IServiceCollection services)
        {
            services
                .AddTransient<TeamSearchRepository>()
                .AddTransient<TeamSearchService>();

            return services;
        }
    }
}
