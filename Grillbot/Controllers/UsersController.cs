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
        public async Task<IActionResult> IndexAsync()
        {
            var guilds = Client.Guilds.ToList();

            var filter = new WebAdminUserListFilter();
            var users = await UserService.GetUsersList(filter);
            var filterUsers = await UserService.GetUsersForFilterAsync();

            var viewModel = new WebAdminUserListViewModel(users, guilds, filter, filterUsers);
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> IndexAsync([FromForm] WebAdminUserListFilter filter)
        {
            var guilds = Client.Guilds.ToList();

            var users = await UserService.GetUsersList(filter);
            var filterUsers = await UserService.GetUsersForFilterAsync();

            var viewModel = new WebAdminUserListViewModel(users, guilds, filter, filterUsers);
            return View(viewModel);
        }

        [HttpGet("UserInfo")]
        public async Task<IActionResult> UserInfoAsync([FromQuery] int userId)
        {
            var user = await UserService.GetUserAsync(userId);
            return View(new WebAdminUserInfoViewModel(user));
        }
    }
}