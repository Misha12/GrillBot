using System;
using System.Security.Claims;

namespace Grillbot.Models.Users
{
    public class WebAdminUserInfoViewModel
    {
        public DiscordUser User { get; set; }

        public bool CanToggleBotAdmin { get; }

        public WebAdminUserInfoViewModel(DiscordUser user, ClaimsPrincipal loggedUser)
        {
            User = user;

            var userId = Convert.ToInt64(loggedUser.FindFirstValue(ClaimTypes.PostalCode));
            var isBotAdmin = loggedUser.FindFirstValue(ClaimTypes.Role) == "BotAdmin";

            CanToggleBotAdmin = userId != user.ID && isBotAdmin;
        }
    }
}
