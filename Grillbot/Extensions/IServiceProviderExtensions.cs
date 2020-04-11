using Grillbot.Database.Repository;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Grillbot.Extensions
{
    public static class IServiceProviderExtensions
    {
        public static ConfigRepository GetConfigRepository(this IServiceProvider provider)
        {
            return provider.GetService<ConfigRepository>();
        }
    }
}
