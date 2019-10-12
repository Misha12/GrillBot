using Discord.WebSocket;
using Grillbot.Services.Auth;
using Grillbot.Services.Config.Models;
using Grillbot.Services.Preconditions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Grillbot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private Configuration Config { get; }
        private DiscordSocketClient Client { get; }
        private AuthService AuthService { get; }

        public AuthController(IOptions<Configuration> options, DiscordSocketClient client, AuthService authService)
        {
            Config = options.Value;
            Client = client;
            AuthService = authService;
        }

        [HttpGet("[action]")]
        public IActionResult GetOAuthUrl(string origin)
        {
            const string endpoint = "https://discordapp.com/api/oauth2/authorize";
            const string scopes = "identify guilds";
            ulong clientID = Config.Discord.ClientId;

            return Ok(new { data = $"{endpoint}?client_id={clientID}&redirect_uri={origin}&response_type=code&scope={scopes}" });
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> Authorize([Required] string code, string origin)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var request = WebRequest.CreateHttp("https://discordapp.com/api/oauth2/token");
            request.Method = "POST";

            string parameters = string.Join("&", new[]
            {
                "client_id=" + Config.Discord.ClientId,
                "client_secret=" + Config.Discord.ClientSecret,
                "grant_type=authorization_code",
                "code=" + code,
                "redirect_uri=" + origin
            });
            byte[] paramsBytes = Encoding.UTF8.GetBytes(parameters);

            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = paramsBytes.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(paramsBytes, 0, paramsBytes.Length);
            }

            try
            {
                using (var response = (HttpWebResponse)await request.GetResponseAsync())
                {
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        var data = JObject.Load(new JsonTextReader(reader));
                        var token = data["access_token"].ToString();

                        var isValid = await GetUserAndValidate(token);

                        if(!isValid)
                        {
                            throw new WebException(JsonConvert.SerializeObject(new
                            {
                                error = "invalid_rights"
                            }));
                        }

                        AuthService.AddToken(token, Convert.ToInt32(data["expires_in"]));
                        return Ok(new { token });
                    }
                }
            }
            catch(ArgumentException ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
            catch (WebException ex)
            {
                if (ex.Response == null)
                    return StatusCode(500, ex.Message);

                using (var reader = new StreamReader(ex.Response.GetResponseStream()))
                {
                    return StatusCode(500, reader.ReadToEnd());
                }
            }
        }

        private async Task<bool> GetUserAndValidate(string token)
        {
            var request = WebRequest.CreateHttp("https://discordapp.com/api/users/@me");
            request.Headers.Add(HttpRequestHeader.Authorization, $"Bearer {token}");

            using (var response = (HttpWebResponse)await request.GetResponseAsync())
            {
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    var data = JObject.Load(new JsonTextReader(reader));
                    var guild = Client.Guilds.First();
                    var user = guild.GetUser(Convert.ToUInt64(data["id"]));

                    if (user == null)
                    {
                        throw new WebException(JsonConvert.SerializeObject(new
                        {
                            error = "not_in_guild"
                        }));
                    }

                    if (Config.IsUserBotAdmin(user.Id))
                        return true;

                    var permissions = Config.MethodsConfig.GetPermissions("GrillStatus");

                    if (permissions.IsUserBanned(user.Id))
                        return false;

                    if (permissions.IsUserAllowed(user.Id))
                        return true;

                    if (user.Roles.Any(o => permissions.IsRoleAllowed(o.Name)))
                        return true;
                        
                    return false;
                }
            }
        }
    }
}