using Grillbot.Repository;
using Microsoft.Extensions.Configuration;

namespace Grillbot.Services
{
    public class TeamSearchService
    {
        private IConfiguration Config { get; }
        public TeamSearchRepository Repository { get; }

        public TeamSearchService(IConfiguration config)
        {
            Config = config;
            Repository = new TeamSearchRepository(Config);
        }
    }
}