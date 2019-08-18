using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Grillbot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SettingsController : ControllerBase
    {
        private IConfiguration Config { get; }

        public SettingsController(IConfiguration config)
        {
            Config = config;
        }

        [HttpGet("[action]")]
        public IActionResult GetCommandPrefix()
        {
            return Ok(new { CommandPrefix = Config["CommandPrefix"] });
        }
    }
}