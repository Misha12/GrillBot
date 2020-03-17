using Discord;
using Grillbot.Services;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace Grillbot.Middleware
{
    public class LogMiddleware
    {
        private RequestDelegate Next { get; }
        private BotLoggingService LoggingService { get; }

        public LogMiddleware(RequestDelegate next, BotLoggingService loggingService)
        {
            Next = next;
            LoggingService = loggingService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var url = GetUrl(context);

            try
            {
                await Next(context).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LoggingService.Write(LogSeverity.Error, url, "", ex);
            }
            finally
            {
                LoggingService.Write(LogSeverity.Info, url, "");
            }
        }

        private string GetUrl(HttpContext context)
        {
            var request = context.Request;

            return string.Concat(request.Scheme, "://", request.Host.ToUriComponent(), request.PathBase.ToUriComponent(),
                request.Path.ToUriComponent(), request.QueryString.ToUriComponent());
        }
    }
}
