using Grillbot.Database.Repository;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;

namespace Grillbot.Services.Permissions
{
    public static class AuthenticationExtensions
    {
        public static IServiceCollection AddWebAuthentication(this IServiceCollection services)
        {
            services
                .AddTransient<WebAuthRepository>()
                .AddTransient<WebAuthenticationService>()
                .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(opt =>
                {
                    opt.LoginPath = "/Login";
                    opt.LogoutPath = "/Logout";
                });

            return services;
        }
    }
}
