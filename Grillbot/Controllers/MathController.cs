using Discord.WebSocket;
using Grillbot.Database.Repository;
using Grillbot.Models.Math;
using Grillbot.Services.Math;
using Grillbot.Services.UserManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        public async Task<IActionResult> AuditAsync(MathAuditLogFilter filter = null)
        {
            var viewModel = await GetAuditViewModelAsync(filter ?? new MathAuditLogFilter());
            return View(viewModel);
        }

        private async Task<MathAuditLogViewModel> GetAuditViewModelAsync(MathAuditLogFilter filter)
        {
            var data = await MathRepository.GetAuditLog(filter)
                .AsAsyncEnumerable()
                .Select(o => new MathAuditItem(o, DiscordClient))
                .ToListAsync();

            var users = await UserService.GetUsersForFilterAsync();
            return new MathAuditLogViewModel(DiscordClient, data, filter, users);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                MathRepository.Dispose();

            base.Dispose(disposing);
        }
    }
}