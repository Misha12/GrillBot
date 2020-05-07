using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Extensions.Discord
{
    public static class SocketGuildExtensions
    {
        public static async Task<List<RestAuditLogEntry>> GetAuditLogDataAsync(this SocketGuild guild, int count = 5, ActionType? actionType = null)
        {
            return (await guild.GetAuditLogsAsync(count, actionType: actionType).FlattenAsync().ConfigureAwait(false)).ToList();
        }

        public static async Task<SocketGuildUser> GetUserFromGuildAsync(this SocketGuild guild, string userIdentification)
        {
            if (ulong.TryParse(userIdentification, out ulong userID))
                return await GetUserFromGuildAsync(guild, userID);

            if(userIdentification.Contains("#"))
            {
                var parts = userIdentification.Split('#');
                return await GetUserFromGuildAsync(guild, parts[0], parts[1]);
            }

            return null;
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

        public static async Task<SocketGuildUser> GetUserFromGuildAsync(this SocketGuild guild, string username, string discriminator)
        {
            bool userFinder(SocketGuildUser user) => user.IsUser() && user.Username == username && user.Discriminator == discriminator;

            var user = guild.Users.FirstOrDefault(userFinder);

            if (user == null)
            {
                await guild.DownloadUsersAsync();
                user = guild.Users.FirstOrDefault(userFinder);
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

        public static int ComputeTextChannelsCount(this SocketGuild guild)
        {
            return guild.Channels.OfType<SocketTextChannel>().Count();
        }

        public static int ComputeVoiceChannelsCount(this SocketGuild guild)
        {
            return guild.Channels.OfType<SocketVoiceChannel>().Count();
        }

        public static SocketRole FindMutedRole(this SocketGuild guild)
        {
            return guild.Roles.FirstOrDefault(o => o.IsMutedRole());
        }
    }
}
