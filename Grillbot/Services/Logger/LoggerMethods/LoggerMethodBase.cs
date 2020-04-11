using Discord.Rest;
using Discord.WebSocket;
using Grillbot.Exceptions;
using Grillbot.Models.Config.AppSettings;
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
        protected Configuration Config { get; }
        protected IMessageCache MessageCache { get; }
        protected HttpClient HttpClient { get; }
        protected ILogger Logger { get; }

        protected LoggerMethodBase(DiscordSocketClient client, Configuration config, IMessageCache messageCache, HttpClient httpClient, ILogger logger)
        {
            Client = client;
            Config = config;
            MessageCache = messageCache;
            HttpClient = httpClient;
            Logger = logger;
        }

        protected ISocketMessageChannel GetLoggerRoom(bool adminChannel = false)
        {
            var id = !adminChannel ? Convert.ToUInt64(Config.Discord.LoggerRoomID) : Config.Discord.AdminChannelSnowflakeID;
            var channel = Client.GetChannel(id);

            if (channel == null)
                throw new BotException($"Cannot find logger room with ID {id}");

            return (ISocketMessageChannel)channel;
        }

        protected async Task<RestUserMessage> SendEmbedAsync(LogEmbedBuilder embedBuilder)
        {
            var loggerRoom = GetLoggerRoom();
            return await loggerRoom.SendMessageAsync(embed: embedBuilder.Build()).ConfigureAwait(false);
        }

        protected async Task<RestUserMessage> SendEmbedToAdminChannel(LogEmbedBuilder embedBuilder)
        {
            var channel = GetLoggerRoom(true);
            return await channel.SendMessageAsync(embed: embedBuilder.Build()).ConfigureAwait(false);
        }
    }
}
