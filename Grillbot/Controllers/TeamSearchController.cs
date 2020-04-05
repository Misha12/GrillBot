using System.Threading.Tasks;
using Grillbot.Models.TeamSearch;
using Grillbot.Services.TeamSearch;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Grillbot.Controllers
{
    [Authorize]
    [Route("/TeamSearch")]
    public class TeamSearchController : Controller
    {
        private TeamSearchService TeamSearchService { get; }

        public TeamSearchController(TeamSearchService teamSearchService)
        {
            TeamSearchService = teamSearchService;
        }

        public async Task<IActionResult> Index()
        {
            var items = await TeamSearchService.GetAllItemsAsync();
            return View(new TeamSearchViewModel(items));
        }
    }
}