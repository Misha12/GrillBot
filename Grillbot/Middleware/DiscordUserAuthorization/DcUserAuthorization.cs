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

            if (headerParts[0].ToLower().Trim() != "GrillBotDcUserAuth")
                throw new UnauthorizedAccessException("Invalid authentication type.");

            var userId = Convert.ToUInt64(headerParts[1]);
            var guild = Client.GetGuild(searchedGuild);

            if (guild == null)
                return null;

            await guild.SyncGuildAsync().ConfigureAwait(false);

            switch (allowedAuthType)
            {
                case DiscordUserAuthorizationType.Everyone:
                    CheckEveryonePermissions(guild, userId);
                    break;
                case DiscordUserAuthorizationType.OnlyOwner:
                    CheckOwnerPermissions(guild, userId);
                    break;
                case DiscordUserAuthorizationType.OnlyBot:
                    CheckBotsPermissions(guild, userId);
                    break;
                case DiscordUserAuthorizationType.WithAdministratorPermission:
                    CheckAdministratorPermissions(guild, userId);
                    break;
            }

            return guild;
        }

        private void CheckEveryonePermissions(SocketGuild guild, ulong userID)
        {
            if(guild.GetUser(userID) == null)
                throw new UnauthorizedAccessException("User defined in authorization header is not on required server.");
        }

        private void CheckOwnerPermissions(SocketGuild guild, ulong userID)
        {
            if (guild.OwnerId != userID)
                throw new UnauthorizedAccessException("This method is allowed only for server owner.");
        }

        private void CheckBotsPermissions(SocketGuild guild, ulong userID)
        {
            CheckEveryonePermissions(guild, userID);

            var user = guild.GetUser(userID);

            if (!user.IsBot)
                throw new UnauthorizedAccessException("This method is allowed only for bot users");
        }

        private void CheckAdministratorPermissions(SocketGuild guild, ulong userID)
        {
            CheckEveryonePermissions(guild, userID);

            if (guild.OwnerId == userID)
                return; // Owner is administrator.

            var user = guild.GetUser(userID);

            if (!user.Roles.Any(o => o.Permissions.Administrator))
                throw new UnauthorizedAccessException("This method is allowed only for Administrators");
        }
    }
}
