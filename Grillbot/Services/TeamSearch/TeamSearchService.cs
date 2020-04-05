using Discord;
using Discord.WebSocket;
using Grillbot.Database.Repository;
using Grillbot.Extensions.Discord;
using Grillbot.Models.TeamSearch;
using Grillbot.Services.MessageCache;
using Microsoft.EntityFrameworkCore.Internal;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Services.TeamSearch
{
    public class TeamSearchService
    {
        private TeamSearchRepository Repository { get; }
        private DiscordSocketClient DiscordClient { get; }
        private IMessageCache MessageCache { get; }

        public TeamSearchService(TeamSearchRepository repository, DiscordSocketClient discordClient, IMessageCache messageCache)
        {
            Repository = repository;
            DiscordClient = discordClient;
            MessageCache = messageCache;
        }

        public async Task<List<TeamSearchItem>> GetItemsAsync(string channelID)
        {
            var items = Repository.GetAllSearches(channelID);
            var data = new List<TeamSearchItem>();

            foreach(var dbItem in items)
            {
                var item = await TransformItemAsync(dbItem);

                if (item != null)
                    data.Add(item);
            }

            return data;
        }

        public async Task<List<TeamSearchItem>> GetAllItemsAsync()
        {
            return await GetItemsAsync(null);
        }

        private async Task<TeamSearchItem> TransformItemAsync(Database.Entity.TeamSearch dbItem)
        {
            // TODO: Refactor. Issue #83.
            if (!(DiscordClient.Guilds
                .FirstOrDefault(o => o.Channels.Any(x => x.Id == dbItem.ChannelIDSnowflake))
                .Channels.FirstOrDefault(o => o.Id == dbItem.ChannelIDSnowflake) is SocketGuildChannel channel))
            {
                Repository.RemoveSearch(dbItem.Id);
                return null;
            }

            var message = await MessageCache.GetAsync(dbItem.ChannelIDSnowflake, dbItem.MessageIDSnowflake);

            if (message == null)
            {
                Repository.RemoveSearch(dbItem.Id);
                return null;
            }

            return new TeamSearchItem()
            {
                ID = dbItem.Id,
                FullUsername = message.Author.GetFullName(),
                ShortUsername = message.Author.GetShortName(),
                Message = message.Content.Substring("hledam add".Length + 1),
                MessageLink = message.GetJumpUrl(),
                ChannelName = channel.Name,
                GuildName = channel.Guild.Name
            };
        }
    }
}
