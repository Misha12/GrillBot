using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Discord.WebSocket;
using Grillbot.Enums;
using Grillbot.Extensions.Discord;
using Grillbot.Models;
using Grillbot.Models.Users;
using Grillbot.Services.Permissions.Api;
using Microsoft.AspNetCore.Mvc;

namespace Grillbot.Controllers.Api
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : Controller
    {
        private DiscordSocketClient DiscordClient { get; }

        public UsersController(DiscordSocketClient discordClient)
        {
            DiscordClient = discordClient;
        }

        [HttpPost("usersSimpleInfoBatch/{guildId}")]
        [DiscordAuthAccessType(AccessType = AccessType.OnlyBot)]
        [ProducesResponseType(typeof(List<SimpleUserInfo>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        // From body is a hack. Because query have length limit.
        public async Task<IActionResult> GetUsersSimpleInfoBatch(ulong guildId, [FromBody] GetUsersSimpleInfoBatchRequest request)
        {
            var guild = DiscordClient.GetGuild(guildId);

            if (guild == null)
                return BadRequest(new { Message = "Requested guild not found." });

            await guild.SyncGuildAsync();

            var users = new List<SimpleUserInfo>();
            foreach(var id in request.UserIDs)
            {
                var user = await guild.GetUserFromGuildAsync(id);

                if (user != null)
                    users.Add(SimpleUserInfo.Create(user));
            }

            return Ok(users);
        }
    }
}
