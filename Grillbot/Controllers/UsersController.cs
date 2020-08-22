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

            var getUsersTask = UserService.GetUsersList(filter);
            var getUsersFilterTask = UserService.GetUsersForFilterAsync();

            await Task.WhenAll(getUsersTask, getUsersFilterTask);

            var viewModel = new WebAdminUserListViewModel(await getUsersTask, guilds, filter, await getUsersFilterTask);
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