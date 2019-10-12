using Microsoft.AspNetCore.Mvc;

namespace Grillbot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BotStatusController : ControllerBase
    {
        private void CheckAuth()
        {
            //TODO
        }

        [HttpGet("[action]")]
        public IActionResult GetSimpleStatus()
        {
            CheckAuth();

            //TODO
            return Ok();
        }

        [HttpGet("[action]")]
        public IActionResult GetCallStats()
        {
            CheckAuth();

            // TODO
            return Ok();
        }

        [HttpGet("[action]")]
        public IActionResult GetLoggerStats()
        {
            CheckAuth();

            // TODO
            return Ok();
        }

        [HttpGet("[action]")]
        public IActionResult GetTempUnverifyLog()
        {
            CheckAuth();

            //TODO
            return Ok();
        }

        [HttpGet("[action]")]
        public IActionResult GetSimpleAutoReplyStats()
        {
            CheckAuth();

            // TODO
            return Ok();
        }
    }
}