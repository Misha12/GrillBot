using Grillbot.Services;
using Grillbot.Services.Auth;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Grillbot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BotStatusController : ControllerBase
    {
        private AuthService AuthService { get; }
        private BotStatusService BotStatusService { get; }

        public BotStatusController(AuthService authService, BotStatusService botStatusService)
        {
            AuthService = authService;
            BotStatusService = botStatusService;
        }

        private bool CheckAuth()
        {
            var authHeader = Request.Headers["Authorization"].ToString();

            if (string.IsNullOrEmpty(authHeader))
                return false;

            return AuthService.IsTokenValid(authHeader);
        }

        [HttpGet("[action]")]
        public IActionResult GetSimpleStatus()
        {
            if (!CheckAuth())
                return Unauthorized();

            var data = BotStatusService.GetSimpleStatus();
            return Ok(data);
        }

        [HttpGet("[action]")]
        public IActionResult GetCallStats()
        {
            if (!CheckAuth())
                return Unauthorized();

            var data = BotStatusService.GetCallStats();
            return Ok(data);
        }

        [HttpGet("[action]")]
        public IActionResult GetLoggerStats()
        {
            if (!CheckAuth())
                return Unauthorized();

            return Ok(BotStatusService.GetLoggerStats());
        }

        [HttpGet("[action]")]
        public IActionResult GetTempUnverifyLog()
        {
            if (!CheckAuth())
                return Unauthorized();

            //TODO
            return Ok();
        }

        [HttpGet("[action]")]
        public IActionResult GetAutoReplyStats()
        {
            if (!CheckAuth())
                return Unauthorized();

            var data = BotStatusService.GetAutoReplyItems();
            return Ok(data);
        }

        [HttpGet("[action]")]
        public IActionResult GetEventStatistics()
        {
            if (!CheckAuth())
                return Unauthorized();

            var data = BotStatusService.GetCalledEventStats();
            return Ok(data);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetCommandLogAsync()
        {
            if (!CheckAuth())
                return Unauthorized();

            var data = await BotStatusService.GetCommandLogsAsync();
            return Ok(data);
        }
    }
}