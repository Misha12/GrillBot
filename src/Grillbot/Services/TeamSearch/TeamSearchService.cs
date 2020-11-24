using Discord;
using Discord.WebSocket;
using Grillbot.Extensions.Discord;
using Grillbot.Models.TeamSearch;
using Grillbot.Services.MessageCache;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Grillbot.Database;
using Microsoft.EntityFrameworkCore;

namespace Grillbot.Services.TeamSearch
{
    public class TeamSearchService
    {
        private DiscordSocketClient DiscordClient { get; }
        private IMessageCache MessageCache { get; }
        private IGrillBotRepository GrillBotRepository { get; }

        private const int MaxSearchSize = 1900;

        public TeamSearchService(IGrillBotRepository grillBotRepository, DiscordSocketClient discordClient, IMessageCache messageCache)
        {
            GrillBotRepository = grillBotRepository;
            DiscordClient = discordClient;
            MessageCache = messageCache;
        }

        public async Task<List<TeamSearchItem>> GetItemsAsync(string channelId)
        {
            var items = await GrillBotRepository.TeamSearchRepository.GetAllSearches(channelId).ToListAsync();
            var data = new List<TeamSearchItem>();

            foreach (var dbItem in items)
            {
                var item = await TransformItemAsync(dbItem);

                if (item != null)
                    data.Add(item);
            }

            await GrillBotRepository.CommitAsync();
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
                GrillBotRepository.Remove(dbItem);
                return null;
            }

            var channel = guild.GetChannel(dbItem.ChannelIDSnowflake);

            if (channel == null)
            {
                GrillBotRepository.Remove(dbItem);
                return null;
            }

            var message = await MessageCache.GetAsync(dbItem.ChannelIDSnowflake, dbItem.MessageIDSnowflake);

            if (IsEmptyMessage(message))
            {
                GrillBotRepository.Remove(dbItem);
                return null;
            }

            return new TeamSearchItem()
            {
                ID = dbItem.Id,
                ShortUsername = message.Author.GetShortName(),
                Message = FormatMessage(message.Content),
                MessageLink = message.GetJumpUrl(),
                ChannelName = channel.Name
            };
        }

        /// <summary>
        /// Checks for non existing messages, empty messages and messages contains only "{prefix}hledam add".
        /// </summary>
        private bool IsEmptyMessage(IMessage message)
        {
            return string.IsNullOrEmpty(message?.Content) || Regex.IsMatch(message.Content, @"(^.)hledam(\s*add)?$");
        }

        /// <summary>
        /// Removes 'hledam' and old 'hledam add' command strings.
        /// </summary>
        private string FormatMessage(string content)
        {
            if (content[1..].StartsWith("hledam add"))
                return content[("hledam add".Length + 1)..].Trim();

            if (content[1..].StartsWith("hledam"))
                return content[("hledam".Length + 1)..].Trim();

            return content.Trim();
        }

        public async Task CreateSearchAsync(SocketGuild guild, SocketUser user, ISocketMessageChannel channel, SocketUserMessage message)
        {
            if (message.Content.Length > MaxSearchSize)
                throw new ValidationException("Zpráva je příliš dlouhá.");

            var entity = new Database.Entity.TeamSearch()
            {
                ChannelIDSnowflake = channel.Id,
                GuildIDSnowflake = guild.Id,
                MessageIDSnowflake = message.Id,
                UserIDSnowflake = user.Id
            };

            await GrillBotRepository.AddAsync(entity);
            await GrillBotRepository.CommitAsync();
        }

        public async Task RemoveSearchAsync(int searchID, SocketGuildUser executor)
        {
            var search = await GrillBotRepository.TeamSearchRepository.FindSearchByIDAsync(searchID);

            if (search == null)
                return;

            var guildPerms = executor.GuildPermissions.Administrator || executor.GuildPermissions.ManageMessages;
            if (!guildPerms && executor.Id != search.UserIDSnowflake)
                throw new UnauthorizedAccessException("Na provedení tohoto příkazu nemáš právo.");

            GrillBotRepository.Remove(search);
            await GrillBotRepository.CommitAsync();
        }

        public async Task<List<string>> BatchCleanAsync(int[] ids)
        {
            var searches = await GrillBotRepository.TeamSearchRepository.GetSearches(ids).ToListAsync();
            return await BatchCleanAsync(searches);
        }

        private async Task<List<string>> BatchCleanAsync(List<Database.Entity.TeamSearch> searches)
        {
            var messages = new List<string>();

            foreach (var search in searches)
            {
                var message = await MessageCache.GetAsync(search.ChannelIDSnowflake, search.MessageIDSnowflake);

                if (message == null)
                    messages.Add($"Smazáno neznámé hledání s ID **{search.Id}**");
                else
                    messages.Add($"Smazáno hledání s ID **{search.Id}** od **{message.Author.GetFullName()}** v **#{message.Channel.Name}**");

                GrillBotRepository.Remove(search);
            }

            await GrillBotRepository.CommitAsync();
            return messages;
        }

        public async Task<List<string>> BatchCleanChannelAsync(ulong channelID)
        {
            var searches = await GrillBotRepository.TeamSearchRepository.GetAllSearches(channelID.ToString()).ToListAsync();
            return await BatchCleanAsync(searches);
        }
    }
}
