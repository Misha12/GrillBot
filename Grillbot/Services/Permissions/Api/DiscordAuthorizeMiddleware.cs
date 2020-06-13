using Grillbot.Extensions.Discord;
using Grillbot.Services.UserManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace Grillbot.Services.Permissions.Api
{
    public class DiscordAuthorizeMiddleware
    {
        private readonly RequestDelegate Next;
        private ILogger<DiscordAuthorizeMiddleware> Logger { get; }
        private UserService UserService { get; }

        public DiscordAuthorizeMiddleware(RequestDelegate next, ILogger<DiscordAuthorizeMiddleware> logger,
            UserService userService)
        {
            Next = next;
            Logger = logger;
            UserService = userService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var endpoint = context.GetEndpoint();

            if (endpoint == null || !endpoint.Metadata.OfType<ApiControllerAttribute>().Any())
            {
                await Next(context);
                return;
            }

            var token = GetAuthorizationToken(context);

            if (string.IsNullOrEmpty(token))
            {
                await SetResponseAsync(context.Response, HttpStatusCode.Unauthorized, "Missing token or invalid format.");
                return;
            }

            Logger.LogInformation($"Requested access to ({context.Request.Path.ToUriComponent()}) with token {token}");
            var action = endpoint.Metadata.OfType<ControllerActionDescriptor>().FirstOrDefault();

            if (action == null)
            {
                await Next(context);
                return;
            }

            var accessType = action.MethodInfo.GetCustomAttributes().OfType<DiscordAuthAccessTypeAttribute>().FirstOrDefault();

            if (accessType == null)
            {
                await SetResponseAsync(context.Response, HttpStatusCode.InternalServerError, "Invalid access type");
                return;
            }
            else if (accessType.AccessType == AccessType.None)
            {
                await SetResponseAsync(context.Response, HttpStatusCode.Forbidden, "This method is disabled.");
                return;
            }

            var user = await UserService.GetUserWithApiTokenAsync(token);

            if (user == null)
            {
                await SetResponseAsync(context.Response, HttpStatusCode.Unauthorized, "Invalid token");
                return;
            }

            await UserService.IncrementApiCallStatistics(token);

            // Types, that requires additional checks.
            switch (accessType.AccessType)
            {
                case AccessType.OnlyBot when user.User.IsUser():
                    await SetResponseAsync(context.Response, HttpStatusCode.Forbidden, "This method is only for bots.");
                    return;
                case AccessType.OnlyOwner when !user.User.IsGuildOwner(user.Guild):
                    await SetResponseAsync(context.Response, HttpStatusCode.Forbidden, "This method is only for guild owners.");
                    return;
                case AccessType.OnlyWithWebAdminAccess when !user.WebAdminAccess:
                    await SetResponseAsync(context.Response, HttpStatusCode.Forbidden, "This method is only for users with web admin access.");
                    return;
            }

            await Next(context);
        }

        private string GetAuthorizationToken(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue("Authorization", out var values))
            {
                var token = values.FirstOrDefault();

                if (string.IsNullOrEmpty(token))
                    return null;

                if (!token.StartsWith("GrillBot"))
                    return null;

                return token.Substring("GrillBot".Length).Trim();
            }

            return null;
        }

        private async Task SetResponseAsync(HttpResponse response, HttpStatusCode code, string text)
        {
            response.Clear();
            response.StatusCode = (int)code;
            await response.WriteAsync(text);
        }
    }
}
