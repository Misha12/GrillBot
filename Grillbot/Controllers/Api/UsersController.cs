using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Grillbot.Enums;
using Grillbot.Exceptions;
using Grillbot.Models;
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

        [HttpPost("usersSimpleInfoBatch/{guild}")]
        [DiscordAuthAccessType(AccessType = AccessType.OnlyBot)]
        [ProducesResponseType(typeof(List<SimpleUserInfo>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        // From body is a hack. Because query have length limit.
        public async Task<IActionResult> GetUsersSimpleInfoBatch(ulong guild, [FromBody] GetUsersSimpleInfoBatchRequest request)
        {
            try
            {
                var data = await UserService.GetSimpleUsersList(guild, request.UserIDs);
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
