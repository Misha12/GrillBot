using Discord.WebSocket;
using Grillbot.Extensions.Discord;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Middleware.DiscordUserAuthorization
{
    public class DcUserAuthorization
    {
        private DiscordSocketClient Client { get; }

        public DcUserAuthorization(DiscordSocketClient client)
        {
            Client = client;
        }

        public async Task<SocketGuild> CheckAuthAndGetGuildAsync(HttpRequest request, DiscordUserAuthorizationType allowedAuthType, ulong searchedGuild)
        {
            var authHeader = request.Headers["Authorization"].ToString();

            if (string.IsNullOrEmpty(authHeader))
                throw new UnauthorizedAccessException("Missing Authorization header");

            var headerParts = authHeader.Split(' ');

            if (headerParts.Length != 2)
                throw new UnauthorizedAccessException("Authorization header is in invalid format.");

            if(!string.Equals(headerParts[0].Trim(), "GrillBotDcUserAuth", StringComparison.InvariantCultureIgnoreCase))
                throw new UnauthorizedAccessException("Invalid authentication type.");

            var userId = Convert.ToUInt64(headerParts[1]);
            var guild = Client.GetGuild(searchedGuild);

            if (guild == null)
                return null;

            await guild.SyncGuildAsync().ConfigureAwait(false);

            switch (allowedAuthType)
            {
                case DiscordUserAuthorizationType.Everyone:
                    await CheckEveryonePermissions(guild, userId);
                    break;
                case DiscordUserAuthorizationType.OnlyOwner:
                    CheckOwnerPermissions(guild, userId);
                    break;
                case DiscordUserAuthorizationType.OnlyBot:
                    await CheckBotsPermissions(guild, userId);
                    break;
                case DiscordUserAuthorizationType.WithAdministratorPermission:
                    await CheckAdministratorPermissions(guild, userId);
                    break;
            }

            return guild;
        }

        private async Task CheckEveryonePermissions(SocketGuild guild, ulong userID)
        {
            var user = await guild.GetUserFromGuildAsync(userID);
            if(user == null)
                throw new ForbiddenAccessException("User defined in authorization header is not on required server.");
        }

        private void CheckOwnerPermissions(SocketGuild guild, ulong userID)
        {
            if (guild.OwnerId != userID)
                throw new ForbiddenAccessException("This method is allowed only for server owner.");
        }

        private async Task CheckBotsPermissions(SocketGuild guild, ulong userID)
        {
            await CheckEveryonePermissions(guild, userID);

            var user = await guild.GetUserFromGuildAsync(userID);

            if (!user.IsUser())
                throw new ForbiddenAccessException("This method is allowed only for bot users");
        }

        private async Task CheckAdministratorPermissions(SocketGuild guild, ulong userID)
        {
            await CheckEveryonePermissions(guild, userID);

            if (guild.OwnerId == userID)
                return; // Owner is administrator.

            var user = await guild.GetUserFromGuildAsync(userID);

            if (!user.Roles.Any(o => o.Permissions.Administrator))
                throw new ForbiddenAccessException("This method is allowed only for Administrators");
        }
    }
}
