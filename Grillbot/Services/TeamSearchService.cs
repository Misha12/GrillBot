using Discord;
using Discord.Commands;
using Grillbot.Repository;
using Grillbot.Services.Config.Models;
using Grillbot.Services.MessageCache;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace Grillbot.Services
{
    public class TeamSearchService : IDisposable
    {
        private Configuration Config { get; }
        public TeamSearchRepository Repository { get; }
        private IMessageCache Cache { get; }

        public TeamSearchService(IOptions<Configuration> config, IMessageCache cache)
        {
            Config = config.Value;
            Repository = new TeamSearchRepository(Config);
            Cache = cache;
        }

        public async Task AddSearchAsync(SocketCommandContext context)
        {
            await Repository.AddSearchAsync(context.User.Id, context.Channel.Id, context.Message.Id);
        }

        public ulong GetGeneralCategoryID() => Config.MethodsConfig.TeamSearch.GeneralCategoryID;

        public async Task<IMessage> GetMessageAsync(ulong channelID, ulong messageID) => await Cache.GetAsync(channelID, messageID);

        public void Dispose()
        {
            Repository.Dispose();
        }
    }
}