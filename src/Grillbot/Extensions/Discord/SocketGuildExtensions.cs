using Discord;
using Discord.Net;
using Discord.Rest;
using Discord.WebSocket;
using Grillbot.Enums;
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

            if (userIdentification.Contains("#"))
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
                await guild.DownloadUsersAsync();
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
                await guild.SyncPromise;

            await guild.DownloadUsersAsync();

            if (guild.DownloaderPromise != null)
                await guild.DownloaderPromise;
        }

        public static int ComputeTextChannelsCount(this SocketGuild guild)
        {
            return guild.Channels.OfType<SocketTextChannel>().Count();
        }

        public static int ComputeVoiceChannelsCount(this SocketGuild guild)
        {
            return guild.Channels.OfType<SocketVoiceChannel>().Count();
        }

        public static async Task<int> CalculateJoinPositionAsync(this SocketGuild guild, SocketGuildUser user)
        {
            await guild.SyncGuildAsync();

            var positions = guild.Users
                .Where(o => o.JoinedAt != null)
                .OrderBy(o => o.JoinedAt)
                .ToList();

            return positions.FindIndex(o => o == user) + 1;
        }

        public static async Task<RestBan> FindBanAsync(this SocketGuild guild, IUser user)
        {
            try
            {
                return await guild.GetBanAsync(user);
            }
            catch (HttpException ex) when (ex.DiscordCode.HasValue && ex.DiscordCode.Value == (int)DiscordJsonCodes.UnknownBan)
            {
                return null;
            }
        }
    }
}
