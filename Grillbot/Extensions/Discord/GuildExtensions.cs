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
            return await GetUserFromGuildAsync(guild, idOfUser);
        }

        public static async Task<SocketGuildUser> GetUserFromGuildAsync(this SocketGuild guild, ulong userId)
        {
            var user = guild.GetUser(userId);

            if (user == null)
            {
                await guild.DownloadUsersAsync().ConfigureAwait(false);
                user = guild.GetUser(userId);
            }

            return user;
        }

        
        public static async Task SyncGuildAsync(this SocketGuild guild)
        {
            if (guild.SyncPromise != null)
                await guild.SyncPromise.ConfigureAwait(false);

            await guild.DownloadUsersAsync().ConfigureAwait(false);

            if (guild.DownloaderPromise != null)
                await guild.DownloaderPromise.ConfigureAwait(false);
        }

        public static async Task<SocketGuildUser> GetUserFromGuildAsync(this SocketGuild guild, string username, string discriminator)
        {
            bool userFinder(SocketGuildUser user) => user.IsUser() && user.Username == username && user.Discriminator == discriminator;

            var user = guild.Users.FirstOrDefault(userFinder);

            if(user == null)
            {
                await guild.DownloadUsersAsync();
                user = guild.Users.FirstOrDefault(userFinder);
            }

            return user;
        }
    }
}
