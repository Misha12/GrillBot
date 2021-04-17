using Microsoft.Extensions.DependencyInjection;
using System;

namespace Grillbot.FileSystem
{
    public static class FileSystemExtensions
    {
        public static IServiceCollection AddFileSystem(this IServiceCollection services, Action<FileSystemContextBuilder> configuration = null)
        {
            services.AddScoped(_ =>
            {
                var builder = new FileSystemContextBuilder();
                if (configuration != null)
                    configuration(builder);
                else
                    builder.Use(Environment.CurrentDirectory);

                return new FileSystemContext(builder);
            });

            services.AddScoped<IFileSystemRepository, FileSystemRepository>();
            return services;
        }
    }
}
