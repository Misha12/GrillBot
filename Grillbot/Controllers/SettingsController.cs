using Grillbot.Services.Config.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Grillbot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SettingsController : ControllerBase
    {
        private Configuration Config { get; }

        public SettingsController(IOptions<Configuration> config)
        {
            Config = config.Value;
        }

        [HttpGet("[action]")]
        public IActionResult GetCommandPrefix() => Ok(new { Config.CommandPrefix });
    }
}