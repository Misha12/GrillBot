using Discord.WebSocket;
using Grillbot.Extensions.Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Services
{
    public class UserSearchService
    {
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
    }
}
