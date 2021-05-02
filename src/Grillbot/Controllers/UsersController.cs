using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Grillbot.Models.Users;
using Grillbot.Services.UserManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Grillbot.Controllers
{
    [Authorize]
    [Route("Admin/Users")]
    public class UsersController : Controller
    {
        private DiscordSocketClient Client { get; }
        private UserService UserService { get; }

        public UsersController(DiscordSocketClient client, UserService userService)
        {
            Client = client;
            UserService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> IndexAsync(WebAdminUserListFilter filter = null)
        {
            if (filter == null)
                filter = new WebAdminUserListFilter();
            if (filter.GuildID == default)
                filter.GuildID = Client.Guilds.FirstOrDefault()?.Id ?? 0;

            var guilds = Client.Guilds.ToList();
            var users = await UserService.GetUsersList(filter);
            var pagination = await UserService.GetPaginationInfo(filter);

            return View(new WebAdminUserListViewModel(users, guilds, filter, pagination));
        }

        [HttpGet("UserInfo")]
        public async Task<IActionResult> UserInfoAsync([FromQuery] long id)
        {
            var user = await UserService.GetUserAsync(id);
            return View(new WebAdminUserInfoViewModel(user));
        }

        [HttpGet("Unblock")]
        public async Task<ActionResult> UnblockAsync([FromQuery] long id)
        {
            await UserService.UnblockUserAsync(id);
            return RedirectToAction("UserInfo", new { id });
        }
    }
}