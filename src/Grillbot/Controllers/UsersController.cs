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
            if (filter.GuildID == default)
                filter.GuildID = Client.Guilds.FirstOrDefault()?.Id ?? 0;

            var guilds = Client.Guilds.ToList();
            var users = await UserService.GetUsersList(filter);
            var pagination = await UserService.CreatePaginationInfo(filter);

            return View(new WebAdminUserListViewModel(users, guilds, filter, pagination));
        }

        [HttpGet("UserInfo")]
        public async Task<IActionResult> UserInfoAsync([FromQuery] int id)
        {
            var user = await UserService.GetCompleteUserInfoAsync(id);
            return View(new WebAdminUserInfoViewModel(user));
        }
    }
}