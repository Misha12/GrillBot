using Discord.WebSocket;
using Grillbot.Database.Repository;
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
        private WebAuthRepository WebAuthRepository { get; }

        public WebAuthenticationService(ILogger<WebAuthenticationService> logger, DiscordSocketClient client,
            WebAuthRepository webAuthRepository)
        {
            Logger = logger;
            DiscordClient = client;
            WebAuthRepository = webAuthRepository;
        }

        public async Task<ClaimsIdentity> Authorize(string username, string password, ulong guildID)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || guildID == default)
                return null;

            SocketGuildUser user;
            try
            {
                var guild = DiscordClient.GetGuild(guildID);

                if (guild == null)
                    return null; // Bot is not in guild.

                var usernameFields = username.Split('#');
                if (usernameFields.Length != 2)
                    return null; // Invalid username format.

                user = await guild.GetUserFromGuildAsync(usernameFields[0], usernameFields[1]);

                if (user == null)
                    return null; // User not found in guild.

                var perm = WebAuthRepository.FindPermById(guild, user);

                if (perm == null || !perm.IsValidPassword(password))
                    return null; // Ban invalid password or undefined permission.

                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.FindHighestRole().Name)
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
