using System.Threading.Tasks;
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

        public UsersController(UserService userService)
        {
            UserService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> IndexAsync(WebAdminUserOrder order, bool desc)
        {
            var users = await UserService.GetUsersList(order, desc);
            return View(new WebAdminUserListViewModel(users, order, desc));
        }

        [HttpGet("UserInfo")]
        public async Task<IActionResult> UserInfoAsync([FromQuery] string userKey)
        {
            var user = await UserService.GetUserAsync(userKey);
            return View(new WebAdminUserInfoViewModel(user));
        }

        protected override void Dispose(bool disposing)
        {
            if(disposing)
            {
                UserService.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}