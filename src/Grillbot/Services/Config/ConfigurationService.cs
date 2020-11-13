using Grillbot.Database.Repository;
using Grillbot.Enums;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace Grillbot.Services.Config
{
    public class ConfigurationService : IDisposable
    {
        private IConfiguration Configuration { get; }
        private GlobalConfigRepository GlobalConfigRepository { get; }

        public ConfigurationService(IConfiguration configuration, GlobalConfigRepository globalConfigRepository)
        {
            Configuration = configuration;
            GlobalConfigRepository = globalConfigRepository;
        }

        public string GetValue(GlobalConfigItems key)
        {
            return Configuration[key.ToString()];
        }

        public async Task SetValueAsync(GlobalConfigItems key, string data)
        {
            Configuration[key.ToString()] = data;
            await GlobalConfigRepository.UpdateItemAsync(key, data);
        }

        public void Dispose()
        {
            GlobalConfigRepository.Dispose();
        }
    }
}
