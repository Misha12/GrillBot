using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Grillbot.Exceptions;
using Grillbot.Models.Users;
using Grillbot.Services.Permissions.Api;
using Grillbot.Services.UserManagement;
using Microsoft.AspNetCore.Mvc;

namespace Grillbot.Controllers.Api
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : Controller
    {
        private UserService UserService { get; }
        public UsersController(UserService userService)
        {
            UserService = userService;
        }

        [HttpGet("usersSimpleInfoBatch/{guild}")]
        [DiscordAuthAccessType(AccessType = AccessType.OnlyBot)]
        [ProducesResponseType(typeof(List<SimpleUserInfo>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetUsersSimpleInfoBatch(ulong guild, [FromQuery] List<ulong> userIds)
        {
            try
            {
                var data = await UserService.GetSimpleUsersList(guild, userIds);
                return Ok(data);
            }
            catch(BadRequestException ex)
            {
                return BadRequest(new
                {
                    ex.Message,
                    Data = ex.Data["Data"]
                });
            }
        }
    }
}
