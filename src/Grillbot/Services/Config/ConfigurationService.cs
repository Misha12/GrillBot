using Grillbot.Database.Repository;
using Grillbot.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Grillbot.Services.Config
{
    public class ConfigurationService
    {
        private IConfiguration Configuration { get; }
        private IServiceProvider ServiceProvider { get; }

        public string Token => Configuration["APP_TOKEN"];

        public ConfigurationService(IConfiguration configuration, IServiceProvider serviceProvider)
        {
            Configuration = configuration;
            ServiceProvider = serviceProvider;
        }

        public string GetValue(GlobalConfigItems key)
        {
            return Configuration[key.ToString()];
        }

        public async Task SetValueAsync(GlobalConfigItems key, string data)
        {
            Configuration[key.ToString()] = data;

            using var repository = ServiceProvider.GetService<GlobalConfigRepository>();
            await repository.UpdateItemAsync(key, data);
        }
    }
}
