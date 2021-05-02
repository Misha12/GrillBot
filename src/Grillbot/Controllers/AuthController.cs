using Discord.WebSocket;
using Grillbot.Models.Auth;
using Grillbot.Services.Permissions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Grillbot.Controllers
{
    [Authorize]
    public class AuthController : Controller
    {
        private WebAuthenticationService WebAuth { get; }
        private DiscordSocketClient DiscordClient { get; }

        public AuthController(WebAuthenticationService webAuth, DiscordSocketClient discordClient)
        {
            WebAuth = webAuth;
            DiscordClient = discordClient;
        }

        [AllowAnonymous]
        [Route("Login")]
        public IActionResult Index()
        {
            return View(new AuthViewModel(DiscordClient.Guilds.ToList()));
        }

        [HttpPost]
        [Route("Login")]
        [AllowAnonymous]
        public async Task<IActionResult> Index([FromQuery(Name = "ReturnUrl")] string returnUrl, string username, string password, ulong guild)
        {
            var identity = await WebAuth.AuthorizeAsync(username, password, guild);

            if (identity == null)
                return View(new AuthViewModel(DiscordClient.Guilds.ToList(), WebAuth.LastLoginResult));

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity), new AuthenticationProperties()
                {
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1),
                    IssuedUtc = DateTimeOffset.UtcNow,
                    RedirectUri = string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl
                });

            return Redirect(string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl);
        }

        [AllowAnonymous]
        [Route("Logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Index");
        }
    }
}