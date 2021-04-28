using Grillbot.Database;
using Grillbot.Enums;
using Grillbot.Extensions.Discord;
using Grillbot.Services.UserManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
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
        private IServiceProvider Provider { get; }

        public DiscordAuthorizeMiddleware(RequestDelegate next, ILogger<DiscordAuthorizeMiddleware> logger, IServiceProvider provider)
        {
            Next = next;
            Logger = logger;
            Provider = provider;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var endpoint = context.GetEndpoint();

            if (endpoint == null || !endpoint.Metadata.OfType<ApiControllerAttribute>().Any())
            {
                await Next(context);
                return;
            }

            var token = GetAuthorizationToken(context)?.ToString();

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

            using var scope = Provider.CreateScope();
            var userService = scope.ServiceProvider.GetService<UserService>();
            var user = await userService.GetUserAsync(token);

            if (user == null)
            {
                await SetResponseAsync(context.Response, HttpStatusCode.Unauthorized, "Invalid token");
                return;
            }

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

        private Guid? GetAuthorizationToken(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue("Authorization", out var values))
            {
                var token = values.FirstOrDefault();

                if (string.IsNullOrEmpty(token))
                    return null;

                if (!token.StartsWith("GrillBot"))
                    return null;

                var tokenData = token["GrillBot".Length..].Trim();
                return Guid.TryParse(tokenData, out Guid result) ? result : (Guid?)null;
            }

            return null;
        }

        private async Task SetResponseAsync(HttpResponse response, HttpStatusCode code, string text)
        {
            response.Clear();
            response.StatusCode = (int)code;
            response.ContentType = "application/json";
            await response.WriteAsync(JsonConvert.SerializeObject(new { Message = text }));
        }
    }
}
