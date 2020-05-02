using Microsoft.Extensions.DependencyInjection;

namespace Grillbot.Services.WebAdmin
{
    public static class WebAdminExtensions
    {
        public static IServiceCollection AddWebAdmin(this IServiceCollection services)
        {
            services.AddTransient<UserService>();

            return services;
        }
    }
}
