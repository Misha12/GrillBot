using System;
using System.Linq;
using System.Threading.Tasks;
using Grillbot.Middleware.DiscordUserAuthorization;
using Grillbot.Services.Statistics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Grillbot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmoteStatsController : ControllerBase
    {
        private DcUserAuthorization Auth { get; }
        private EmoteStats EmoteStats { get; }

        public EmoteStatsController(DcUserAuthorization auth, EmoteStats emoteStats)
        {
            Auth = auth;
            EmoteStats = emoteStats;
        }

        [HttpGet("getAll/{guildID}")]
        public async Task<IActionResult> GetAll(ulong guildID, [FromQuery] int limit = 25, bool withUnicode = false)
        {
            try
            {
                var guild = await Auth.CheckAuthAndGetGuildAsync(HttpContext.Request, DiscordUserAuthorizationType.Everyone, guildID).ConfigureAwait(false);

                if (guild == null)
                    return NotFound();

                var data = EmoteStats.GetAllValues(true, guild.Id, !withUnicode)
                    .Take(limit)
                    .Select(o => new
                    {
                        emote = o.GetRealId(),
                        count = o.Count,
                        lastOccuredAt = o.LastOccuredAt,
                        isUnicode = o.IsUnicode
                    });

                return Ok(data.ToList());
            }
            catch(UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch(ForbiddenAccessException ex)
            {
                return StatusCode(403, ex.Message);
            }
        }
    }
}