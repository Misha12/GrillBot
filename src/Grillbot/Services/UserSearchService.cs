using Discord;
using Discord.WebSocket;
using Grillbot.Database.Repository;
using Grillbot.Extensions.Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Services
{
    public class UserSearchService : IDisposable
    {
        private BotState BotState { get; }
        private UsersRepository UsersRepository { get; }
        
        public UserSearchService(BotState botState, UsersRepository usersRepository)
        {
            BotState = botState;
            UsersRepository = usersRepository;
        }

        public async Task<List<SocketGuildUser>> FindUsersAsync(SocketGuild guild, string query)
        {
            if (string.IsNullOrEmpty(query))
                return new List<SocketGuildUser>();

            await guild.SyncGuildAsync();

            if (query.Equals("*", StringComparison.InvariantCultureIgnoreCase))
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

            var id = await UsersRepository.FindUserIDFromDiscordIDAsync(guild.Id, user.Id);

            if (id == null)
                return null;

            BotState.UserToID.Add(key, id.Value);
            return id;
        }

        public void Dispose()
        {
            UsersRepository?.Dispose();
        }
    }
}
