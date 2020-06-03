using Discord.WebSocket;
using Grillbot.Database.Repository;
using Grillbot.Models.Math;
using Grillbot.Services.Math;
using Grillbot.Services.UserManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Controllers
{
    [Authorize]
    [Route("Admin/Math")]
    public class MathController : Controller
    {
        private MathService MathService { get; }
        private MathRepository MathRepository { get; }
        private DiscordSocketClient DiscordClient { get; }
        private UserService UserService { get; }

        public MathController(MathService mathService, MathRepository mathRepository, DiscordSocketClient discordClient,
            UserService userService)
        {
            MathService = mathService;
            MathRepository = mathRepository;
            DiscordClient = discordClient;
            UserService = userService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var viewModel = new MathViewModel(MathService.Sessions);
            return View(viewModel);
        }

        [HttpGet("Audit")]
        public async Task<IActionResult> AuditAsync()
        {
            var filter = new MathAuditLogFilter();
            var data = MathRepository.GetAuditLog(filter).ToList();
            var items = data.Select(o => new MathAuditItem(o, DiscordClient)).ToList();
            var users = await UserService.GetUsersForFilterAsync();
            var viewModel = new MathAuditLogViewModel(DiscordClient, items, filter, users);

            return View(viewModel);
        }

        [HttpPost("Audit")]
        public async Task<IActionResult> AuditAsync(MathAuditLogFilter filter)
        {
            var data = MathRepository.GetAuditLog(filter).ToList();
            var items = data.Select(o => new MathAuditItem(o, DiscordClient)).ToList();
            var users = await UserService.GetUsersForFilterAsync();
            var viewModel = new MathAuditLogViewModel(DiscordClient, items, filter, users);

            return View(viewModel);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                MathRepository.Dispose();

            base.Dispose(disposing);
        }
    }
}