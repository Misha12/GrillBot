using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Extensions.Discord
{
    public static class GuildExtensions
    {
        public static async Task<List<RestAuditLogEntry>> GetAuditLogDataAsync(this SocketGuild guild, int count = 5)
        {
            return (await guild.GetAuditLogsAsync(count).FlattenAsync().ConfigureAwait(false)).ToList();
        }

        public static async Task<SocketGuildUser> GetUserFromGuildAsync(this SocketGuild guild, string userId)
        {
            var idOfUser = Convert.ToUInt64(userId);
            var user = guild.GetUser(idOfUser);

            if (user == null)
            {
                await guild.DownloadUsersAsync().ConfigureAwait(false);
                user = guild.GetUser(idOfUser);
            }

            return user;
        }
    }
}
