using Grillbot.Models.Internal;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Grillbot.Controllers
{
    [Authorize]
    [Route("Admin/Internal")]
    public class InternalController : Controller
    {
        [HttpGet("Memory")]
        public IActionResult Memory()
        {
            var viewModel = new MemoryStatusViewModel();
            return View(viewModel);
        }
    }
}