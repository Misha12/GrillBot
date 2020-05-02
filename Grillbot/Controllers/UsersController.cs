using System.Threading.Tasks;
using Grillbot.Models.Users;
using Grillbot.Services.WebAdmin;
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
        public async Task<IActionResult> IndexAsync()
        {
            var users = await UserService.GetUsersList();
            return View(new WebAdminUserListViewModel(users));
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