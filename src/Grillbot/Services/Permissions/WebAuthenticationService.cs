using Discord.WebSocket;
using Grillbot.Extensions.Discord;
using Grillbot.Services.UserManagement;
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
        private UserService UserService { get; }

        public WebAuthenticationService(ILogger<WebAuthenticationService> logger, DiscordSocketClient client, UserService userService)
        {
            Logger = logger;
            DiscordClient = client;
            UserService = userService;
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

                var userID = await UserService.AuthenticateWebAccessAsync(guild, user, password);
                if (userID == null)
                    return null; // Invalid password, or unallowed access.

                await UserService.IncrementWebAdminLoginCountAsync(userID.Value);

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
    }
}
