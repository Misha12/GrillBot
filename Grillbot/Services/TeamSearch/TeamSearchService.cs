using Discord;
using Discord.WebSocket;
using Grillbot.Database.Repository;
using Grillbot.Exceptions;
using Grillbot.Extensions.Discord;
using Grillbot.Models.TeamSearch;
using Grillbot.Services.MessageCache;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grillbot.Services.TeamSearch
{
    public class TeamSearchService : IDisposable
    {
        private TeamSearchRepository Repository { get; }
        private DiscordSocketClient DiscordClient { get; }
        private IMessageCache MessageCache { get; }

        private const int MaxSearchSize = 1900;

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

            foreach (var dbItem in items)
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
            var guild = DiscordClient.GetGuild(dbItem.GuildIDSnowflake);

            if (guild == null)
            {
                Repository.RemoveSearch(dbItem.Id);
                return null;
            }

            var channel = guild.GetChannel(dbItem.ChannelIDSnowflake);

            if (channel == null)
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

        public void CreateSearch(SocketGuild guild, SocketUser user, ISocketMessageChannel channel, SocketUserMessage message)
        {
            if (message.Content.Length > MaxSearchSize)
                throw new BotCommandInfoException("Zpráva je příliš dlouhá.");

            Repository.AddSearch(guild.Id, user.Id, channel.Id, message.Id);
        }

        public void RemoveSearch(int searchID, SocketGuildUser executor)
        {
            var search = Repository.FindSearchByID(searchID);

            if (search == null)
                throw new BotCommandInfoException("Hledaná zpráva neexistuje.");

            var guildPerms = executor.GuildPermissions.Administrator || executor.GuildPermissions.ManageMessages;
            if (!guildPerms && executor.Id != search.UserIDSnowflake)
                throw new BotCommandInfoException("Na provedení tohoto příkazu nemáš právo.");

            Repository.RemoveSearch(search);
        }

        public async Task BatchCleanAsync(int[] ids, Func<string, Task> reply)
        {
            var searches = Repository.GetSearches(ids);
            await BatchCleanAsync(searches, reply);
        }

        private async Task BatchCleanAsync(List<Database.Entity.TeamSearch> searches, Func<string, Task> reply)
        {
            foreach (var search in searches)
            {
                var message = await MessageCache.GetAsync(search.ChannelIDSnowflake, search.MessageIDSnowflake);

                if (message == null)
                    await reply($"Mažu neznámé hledání s ID **{search.Id}**");
                else
                    await reply($"Mažu hledání s ID **{search.Id}** od **{message.Author.GetFullName()}**");

                Repository.RemoveSearch(search);
            }
        }

        public async Task BatchCleanChannelAsync(ulong channelID, Func<string, Task> reply)
        {
            var searches = Repository.GetAllSearches(channelID.ToString());
            await BatchCleanAsync(searches, reply);
        }

        public void Dispose()
        {
            Repository.Dispose();
        }
    }
}
