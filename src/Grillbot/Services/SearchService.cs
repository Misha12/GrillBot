using Discord;
using Discord.WebSocket;
using Grillbot.Database;
using Grillbot.Extensions.Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Services
{
    public class SearchService
    {
        private BotState BotState { get; }
        private IGrillBotRepository GrillBotRepository { get; }

        public SearchService(BotState botState, IGrillBotRepository grillBotRepository)
        {
            BotState = botState;
            GrillBotRepository = grillBotRepository;
        }

        public async Task<List<SocketGuildUser>> FindUsersAsync(SocketGuild guild, string query)
        {
            if (string.IsNullOrEmpty(query))
                return null;

            await guild.SyncGuildAsync();

            if (IsAllWildcard(query))
                return guild.Users.ToList();

            return guild.Users.Where(o => IsValidUserWithQuery(o, query)).ToList();
        }

        private bool IsValidUserWithQuery(SocketGuildUser user, string query)
        {
            if (!string.IsNullOrEmpty(user.Nickname) && user.Nickname.Contains(query))
                return true;

            return user.Username.Contains(query);
        }

        public async Task<long?> GetUserIDFromDiscordUserAsync(IGuild guild, IUser user)
        {
            var key = $"{guild.Id}|{user.Id}";

            if (BotState.UserToID.ContainsKey(key))
                return BotState.UserToID[key];

            var id = await GrillBotRepository.UsersRepository.FindUserIDFromDiscordIDAsync(guild.Id, user.Id);

            if (id == null)
                return null;

            BotState.UserToID.TryAdd(key, id.Value);
            return id;
        }

        public async Task<Dictionary<SocketGuildUser, long?>> ConvertUsersToIDsAsync(IEnumerable<SocketGuildUser> users)
        {
            if (users == null)
                return null;

            var result = new Dictionary<SocketGuildUser, long?>();

            foreach (var user in users)
            {
                var userId = await GetUserIDFromDiscordUserAsync(user.Guild, user);
                result.Add(user, userId);
            }

            return result;
        }

        public async Task<IEnumerable<SocketGuildChannel>> FindChannelsAsync(SocketGuild guild, string query)
        {
            if (string.IsNullOrEmpty(query))
                return null;

            await guild.SyncGuildAsync();
            var channelsQuery = guild.Channels.Where(o => o is SocketTextChannel || o is SocketVoiceChannel);

            if (IsAllWildcard(query))
                return channelsQuery;

            return channelsQuery.Where(o => o.Name.Contains(query));
        }

        private bool IsAllWildcard(string query)
        {
            return query.Equals("*", StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
