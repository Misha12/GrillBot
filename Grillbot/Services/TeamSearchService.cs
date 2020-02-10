using Discord;
using Discord.Commands;
using Grillbot.Database;
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
        public GrillBotRepository Repository { get; }
        private IMessageCache Cache { get; }

        public TeamSearchService(IOptions<Configuration> config, IMessageCache cache)
        {
            Config = config.Value;
            Repository = new GrillBotRepository(Config);
            Cache = cache;
        }

        public async Task AddSearchAsync(SocketCommandContext context)
        {
            await Repository.TeamSearch.AddSearchAsync(context.User.Id, context.Channel.Id, context.Message.Id).ConfigureAwait(false);
        }

        public async Task<IMessage> GetMessageAsync(ulong channelID, ulong messageID) => await Cache.GetAsync(channelID, messageID).ConfigureAwait(false);

        public void Dispose()
        {
            Repository.Dispose();
        }
    }
}