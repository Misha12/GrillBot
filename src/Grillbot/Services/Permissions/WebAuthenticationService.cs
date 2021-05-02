using Discord.WebSocket;
using Grillbot.Database;
using Grillbot.Database.Enums.Includes;
using Grillbot.Enums;
using Grillbot.Extensions.Discord;
using Grillbot.Models.Config.AppSettings;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Grillbot.Services.Permissions
{
    public class WebAuthenticationService
    {
        private ILogger<WebAuthenticationService> Logger { get; }
        private DiscordSocketClient DiscordClient { get; }
        private IGrillBotRepository GrillBotRepository { get; }
        private WebAdminConfiguration Config { get; }

        public WebAdminLoginResult LastLoginResult { get; private set; }

        public WebAuthenticationService(ILogger<WebAuthenticationService> logger, DiscordSocketClient client, IGrillBotRepository grillBotRepository,
            IOptionsSnapshot<WebAdminConfiguration> configuration)
        {
            Logger = logger;
            DiscordClient = client;
            GrillBotRepository = grillBotRepository;
            Config = configuration.Value;
        }

        public async Task<ClaimsIdentity> AuthorizeAsync(string username, string password, ulong guildID)
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

                var loginResult = await CheckLoginAsync(guild, user, password);
                LastLoginResult = loginResult.Item1;
                if (loginResult.Item1 != WebAdminLoginResult.Success)
                    return null; // Invalid password, banned account, ... see details in LastLoginResult property.

                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.GetShortName()),
                    loginResult.Item3 ? new Claim(ClaimTypes.Role, "BotAdmin") : null,
                    new Claim(ClaimTypes.Role, user.FindHighestRole().Name),
                    new Claim(ClaimTypes.UserData, guild.Id.ToString()),
                    new Claim(ClaimTypes.PostalCode, loginResult.Item2.Value.ToString())
                }.Where(o => o != null).ToArray();

                return new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "");
                return null;
            }
        }

        public async Task<Tuple<WebAdminLoginResult, long?, bool>> CheckLoginAsync(SocketGuild guild, SocketGuildUser user, string password)
        {
            try
            {
                var entity = await GrillBotRepository.UsersRepository.GetUserAsync(guild.Id, user.Id, UsersIncludes.None);

                if (string.IsNullOrEmpty(entity?.WebAdminPassword))
                    return new Tuple<WebAdminLoginResult, long?, bool>(WebAdminLoginResult.InvalidLogin, null, false);

                if (entity.WebAdminBannedTo != null)
                {
                    // Banned because too many invalid logins.
                    if (entity.WebAdminBannedTo >= DateTime.Now)
                        return new Tuple<WebAdminLoginResult, long?, bool>(WebAdminLoginResult.BannedAccount, null, false);

                    entity.WebAdminBannedTo = null;
                    entity.FailedLoginCount = 0;
                }

                if (!BCrypt.Net.BCrypt.Verify(password, entity.WebAdminPassword))
                {
                    entity.FailedLoginCount++;

                    if (entity.FailedLoginCount >= Config.MaxFailedCount)
                    {
                        entity.WebAdminBannedTo = DateTime.Now.AddHours(Config.BanHours);
                        return new Tuple<WebAdminLoginResult, long?, bool>(WebAdminLoginResult.BannedAccount, null, false);
                    }

                    return new Tuple<WebAdminLoginResult, long?, bool>(WebAdminLoginResult.InvalidLogin, null, false);
                }

                entity.FailedLoginCount = 0;
                entity.WebAdminLoginCount = (entity.WebAdminLoginCount ?? 0) + 1;

                return new Tuple<WebAdminLoginResult, long?, bool>(WebAdminLoginResult.Success, entity.ID, entity.IsBotAdmin);
            }
            finally
            {
                await GrillBotRepository.CommitAsync();
            }
        }
    }
}
