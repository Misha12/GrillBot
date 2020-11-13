using Discord.Rest;
using Discord.WebSocket;
using Grillbot.Enums;
using Grillbot.Exceptions;
using Grillbot.Services.Config;
using Grillbot.Services.Logger.LoggerMethods.LogEmbed;
using Grillbot.Services.MessageCache;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Grillbot.Services.Logger.LoggerMethods
{
    public abstract class LoggerMethodBase
    {
        protected DiscordSocketClient Client { get; }
        protected IMessageCache MessageCache { get; }
        protected HttpClient HttpClient { get; }
        protected ILogger Logger { get; }
        protected ConfigurationService ConfigurationService { get; }

        protected LoggerMethodBase(DiscordSocketClient client, IMessageCache messageCache, HttpClient httpClient, ILogger logger, ConfigurationService configurationService)
        {
            Client = client;
            MessageCache = messageCache;
            HttpClient = httpClient;
            Logger = logger;
            ConfigurationService = configurationService;
        }

        protected ISocketMessageChannel GetLoggerRoom(bool adminChannel = false)
        {
            var type = !adminChannel ? GlobalConfigItems.LoggerChannel : GlobalConfigItems.AdminChannel;
            var id = ConfigurationService.GetValue(type);

            if (id == null)
                return null;

            var channel = Client.GetChannel(Convert.ToUInt64(id));

            if (channel == null)
                throw new BotException($"Cannot find logger room with ID {id}");

            return (ISocketMessageChannel)channel;
        }

        protected async Task<RestUserMessage> SendEmbedAsync(LogEmbedBuilder embedBuilder)
        {
            var loggerRoom = GetLoggerRoom();

            if (loggerRoom == null)
                return null;

            return await loggerRoom.SendMessageAsync(embed: embedBuilder.Build()).ConfigureAwait(false);
        }

        protected async Task<RestUserMessage> SendEmbedToAdminChannel(LogEmbedBuilder embedBuilder)
        {
            var channel = GetLoggerRoom(true);
            return await channel.SendMessageAsync(embed: embedBuilder.Build()).ConfigureAwait(false);
        }
    }
}
