using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Services.MemeImages
{
    public static class MemeImagesExtensions
    {
        public static IServiceCollection AddMemeImages(this IServiceCollection services)
        {
            services
                .AddTransient<MemeImagesService>();

            return services;
        }
    }
}
