using Discord.Commands;
using Grillbot.Repository;
using Grillbot.Services.Config.Models;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace Grillbot.Services
{
    public class TeamSearchService : IDisposable
    {
        private Configuration Config { get; }
        public TeamSearchRepository Repository { get; }

        public TeamSearchService(IOptions<Configuration> config)
        {
            Config = config.Value;
            Repository = new TeamSearchRepository(Config);
        }

        public async Task AddSearchAsync(SocketCommandContext context)
        {
            await Repository.AddSearchAsync(context.User.Id, context.Channel.Id, context.Message.Id);
        }

        public ulong GetGeneralCategoryID() => Config.MethodsConfig.TeamSearch.GeneralCategoryID;

        public void Dispose()
        {
            Repository.Dispose();
        }
    }
}