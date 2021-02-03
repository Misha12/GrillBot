using Grillbot.Database;
using Grillbot.Database.Entity.Config;
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

        public string Token => Configuration["Token"];

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

            using var scope = ServiceProvider.CreateScope();
            using var repository = scope.ServiceProvider.GetService<IGrillBotRepository>();

            var item = await repository.GlobalConfigRepository.GetItemAsync(key);

            if (item == null)
            {
                item = new GlobalConfigItem()
                {
                    Key = key.ToString(),
                    Value = data
                };

                await repository.AddAsync(item);
            }
            else
            {
                item.Value = data;
            }

            await repository.CommitAsync();
        }
    }
}
