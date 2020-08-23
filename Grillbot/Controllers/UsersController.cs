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
        private UserService UserService { get; }
        private DiscordSocketClient Client { get; }

        public UsersController(UserService userService, DiscordSocketClient client)
        {
            UserService = userService;
            Client = client;
        }

        [HttpGet]
        public async Task<IActionResult> IndexAsync(WebAdminUserListFilter filter = null)
        {
            if (filter == null)
                filter = new WebAdminUserListFilter();

            var guilds = Client.Guilds.ToList();

            var users = await UserService.GetUsersList(filter);
            var usersForFilter = await UserService.GetUsersForFilterAsync();

            var viewModel = new WebAdminUserListViewModel(users, guilds, filter, usersForFilter);
            return View(viewModel);
        }

        [HttpGet("UserInfo")]
        public async Task<IActionResult> UserInfoAsync([FromQuery] int userId)
        {
            var user = await UserService.GetUserInfoAsync(userId, true);
            return View(new WebAdminUserInfoViewModel(user));
        }
    }
}