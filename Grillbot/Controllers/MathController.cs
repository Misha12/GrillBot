using Grillbot.Models.Math;
using Grillbot.Services.Math;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Grillbot.Controllers
{
    [Authorize]
    public class MathController : Controller
    {
        private MathService MathService { get; }

        public MathController(MathService mathService)
        {
            MathService = mathService;
        }

        [Route("Math")]
        public IActionResult Index()
        {
            return View(new MathViewModel(MathService.Sessions));
        }
    }
}