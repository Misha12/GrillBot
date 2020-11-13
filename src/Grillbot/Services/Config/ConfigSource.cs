using Microsoft.Extensions.Configuration;
using System;

namespace Grillbot.Services.Config
{
    public class ConfigSource : IConfigurationSource
    {
        private string ConnectionString { get; }
        public string Mode { get; }

        public ConfigSource(string connectionString, string mode)
        {
            ConnectionString = connectionString;
            Mode = mode;
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return Mode.ToLower() switch
            {
                "global" => new GlobalConfigProvider(ConnectionString),
                _ => throw new NotSupportedException()
            };
        }
    }
}
