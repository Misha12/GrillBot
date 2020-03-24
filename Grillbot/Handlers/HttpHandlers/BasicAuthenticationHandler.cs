using Discord;
using Discord.WebSocket;
using Grillbot.Extensions.Discord;
using Grillbot.Services.Config.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Grillbot.Handlers.HttpHandlers
{
    public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private Configuration Config { get; }
        private DiscordSocketClient Client { get; }

        public BasicAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder,
            ISystemClock clock, DiscordSocketClient client, IOptions<Configuration> config) : base(options, logger, encoder, clock)
        {
            Config = config.Value;
            Client = client;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
                return AuthenticateResult.Fail("Missing Authorization Header");

            SocketGuildUser user;
            try
            {
                var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
                var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
                var credentials = Encoding.UTF8.GetString(credentialBytes).Split(new[] { ':' }, 2);

                var guild = Client.Guilds.FirstOrDefault(o => o.Name == credentials[0]);

                if (guild == null)
                    return AuthenticateResult.Fail("Invalid guild");

                var usernameFields = credentials[1].Split('#');
                if (usernameFields.Length != 2)
                    return AuthenticateResult.Fail("Invalid username format");

                user = await guild.GetUserFromGuildAsync(usernameFields[0], usernameFields[1]);

                if (user == null)
                    return AuthenticateResult.Fail("Invalid user");

                if (!Config.IsUserBotAdmin(user.Id))
                    return AuthenticateResult.Fail("This user is not bot administrator");

                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.FindHighestRole().Name)
                };

                var identity = new ClaimsIdentity(claims, Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);

                return AuthenticateResult.Success(ticket);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "");
                return AuthenticateResult.Fail("An error occured during user authentication");
            }
        }

        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Response.StatusCode = 401;

            var headerValue = $"Basic realm=\"Requested authorization.\"";
            Response.Headers.Append(HeaderNames.WWWAuthenticate, headerValue);

            return Task.CompletedTask;
        }
    }
}
