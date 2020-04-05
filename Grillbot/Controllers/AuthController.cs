using Grillbot.Models;
using Grillbot.Services.Permissions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Grillbot.Controllers
{
    [Authorize]
    public class AuthController : Controller
    {
        private WebAuthenticationService WebAuth { get; }

        public AuthController(WebAuthenticationService webAuth)
        {
            WebAuth = webAuth;
        }

        [AllowAnonymous]
        [Route("Login")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Route("Login")]
        [AllowAnonymous]
        public async Task<IActionResult> Index([FromQuery(Name = "ReturnUrl")] string returnUrl, string username, string password)
        {
            var identity = await WebAuth.Authorize(username, password);

            if (identity == null)
                return View(new AuthViewModel() { InvalidLogin = true });

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity), new AuthenticationProperties()
                {
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1),
                    IssuedUtc = DateTimeOffset.UtcNow,
                    RedirectUri = string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl
                });

            return View();
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