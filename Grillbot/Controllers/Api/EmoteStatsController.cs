using System.Linq;
using System.Threading.Tasks;
using Grillbot.Enums;
using Grillbot.Services.Permissions.Api;
using Grillbot.Services.Statistics;
using Microsoft.AspNetCore.Mvc;

namespace Grillbot.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmoteStatsController : ControllerBase
    {
        private EmoteStats EmoteStats { get; }

        public EmoteStatsController(EmoteStats emoteStats)
        {
            EmoteStats = emoteStats;
        }

        [HttpGet("getAll/{guildID}")]
        [DiscordAuthAccessType(AccessType = AccessType.Everyone)]
        public async Task<IActionResult> GetAll(ulong guildID, [FromQuery] int limit = 25, [FromQuery] bool withUnicode = false)
        {
            var data = EmoteStats.GetAllValues(true, guildID, !withUnicode, limit)
                    .Select(o => new
                    {
                        Emote = o.RealID,
                        o.UseCount,
                        o.FirstOccuredAt,
                        o.LastOccuredAt,
                        o.IsUnicode,
                        o.UsersCount
                    });

            return Ok(data);
        }
    }
}