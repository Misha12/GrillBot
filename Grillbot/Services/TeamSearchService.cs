using Discord.Commands;
using Grillbot.Repository;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace Grillbot.Services
{
    public class TeamSearchService : IDisposable
    {
        private IConfiguration Config { get; }
        public TeamSearchRepository Repository { get; }

        public TeamSearchService(IConfiguration config)
        {
            Config = config;
            Repository = new TeamSearchRepository(Config);
        }

        public async Task AddSearchAsync(SocketCommandContext context)
        {
            await Repository.AddSearchAsync(context.User.Id, context.Channel.Id, context.Message.Id);
        }

        public ulong GetGeneralChannelID() => Convert.ToUInt64(Config["MethodsConfig:TeamSearch:GeneralID"]);

        public void Dispose()
        {
            Repository.Dispose();
        }
    }
}