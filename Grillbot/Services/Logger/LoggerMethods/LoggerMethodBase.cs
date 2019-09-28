using Discord.WebSocket;
using Grillbot.Exceptions;
using Grillbot.Services.Config.Models;
using Grillbot.Services.MessageCache;
using System;
using System.Net.Http;

namespace Grillbot.Services.Logger.LoggerMethods
{
    public abstract class LoggerMethodBase
    {
        protected DiscordSocketClient Client { get; }
        protected Configuration Config { get; }
        protected IMessageCache MessageCache { get; }
        protected HttpClient HttpClient { get; }

        protected LoggerMethodBase(DiscordSocketClient client, Configuration config, IMessageCache messageCache, HttpClient httpClient)
        {
            Client = client;
            Config = config;
            MessageCache = messageCache;
            HttpClient = httpClient;
        }

        protected ISocketMessageChannel GetLoggerRoom()
        {
            var id = Convert.ToUInt64(Config.Discord.LoggerRoomID);
            var channel = Client.GetChannel(id);

            if (channel == null)
                throw new BotException($"Cannot find logger room with ID {id}");

            return (ISocketMessageChannel)channel;
        }
    }
}
