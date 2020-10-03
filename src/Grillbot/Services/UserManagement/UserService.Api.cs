using Grillbot.Exceptions;
using Grillbot.Extensions.Discord;
using Grillbot.Models.Users;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grillbot.Services.UserManagement
{
    public partial class UserService
    {
        public async Task<List<SimpleUserInfo>> GetSimpleUsersList(ulong guildID, List<ulong> userIds)
        {
            var guild = DiscordClient.GetGuild(guildID);

            if (guild == null)
                throw new BadRequestException("Requested guild not found.", new { guildID });

            await guild.SyncGuildAsync();

            var users = new List<SimpleUserInfo>();
            foreach (var id in userIds)
            {
                var user = await guild.GetUserFromGuildAsync(id);

                if (user != null)
                    users.Add(SimpleUserInfo.Create(user));
            }

            return users;
        }
    }
}
