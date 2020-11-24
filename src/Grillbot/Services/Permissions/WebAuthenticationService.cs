using Discord.WebSocket;
using Grillbot.Database;
using Grillbot.Database.Enums.Includes;
using Grillbot.Extensions.Discord;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Grillbot.Services.Permissions
{
    public class WebAuthenticationService
    {
        private ILogger<WebAuthenticationService> Logger { get; }
        private DiscordSocketClient DiscordClient { get; }
        private IGrillBotRepository GrillBotRepository { get; }

        public WebAuthenticationService(ILogger<WebAuthenticationService> logger, DiscordSocketClient client, IGrillBotRepository grillBotRepository)
        {
            Logger = logger;
            DiscordClient = client;
            GrillBotRepository = grillBotRepository;
        }

        public async Task<ClaimsIdentity> Authorize(string username, string password, ulong guildID)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || guildID == default)
                return null;

            try
            {
                var guild = DiscordClient.GetGuild(guildID);

                if (guild == null)
                    return null; // Bot is not in guild.

                var usernameFields = username.Split('#');
                if (usernameFields.Length != 2)
                    return null; // Invalid username format.

                var user = await guild.GetUserFromGuildAsync(usernameFields[0], usernameFields[1]);

                if (user == null)
                    return null; // User not found in guild.

                var userId = await VerifyPasswordAndGetUserIdAsync(guild, user, password);
                if (userId == null)
                    return null; // Invalid password, or unallowed access.

                await IncrementWebAdminStatsAsync(userId.Value);

                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.GetShortName()),
                    new Claim(ClaimTypes.Role, user.FindHighestRole().Name),
                    new Claim(ClaimTypes.UserData, guild.Id.ToString())
                };

                return new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "");
                return null;
            }
        }

        private async Task IncrementWebAdminStatsAsync(long userId)
        {
            var user = await GrillBotRepository.UsersRepository.GetUserAsync(userId, UsersIncludes.None);

            if (user == null)
                return;

            if (user.WebAdminLoginCount == null)
                user.WebAdminLoginCount = 1;
            else
                user.WebAdminLoginCount++;

            await GrillBotRepository.CommitAsync();
        }

        public async Task<long?> VerifyPasswordAndGetUserIdAsync(SocketGuild guild, SocketGuildUser user, string password)
        {
            var entity = await GrillBotRepository.UsersRepository.GetUserAsync(guild.Id, user.Id, UsersIncludes.None);

            if (string.IsNullOrEmpty(entity?.WebAdminPassword) || !BCrypt.Net.BCrypt.Verify(password, entity.WebAdminPassword))
                return null;

            return entity.ID;
        }
    }
}
