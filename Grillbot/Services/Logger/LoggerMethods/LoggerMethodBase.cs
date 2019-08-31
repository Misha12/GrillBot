using Discord.WebSocket;
using Grillbot.Exceptions;
using Grillbot.Services.MessageCache;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Services.Logger.LoggerMethods
{
    public abstract class LoggerMethodBase
    {
        protected DiscordSocketClient Client { get; }
        protected IConfiguration Config { get; }
        protected IMessageCache MessageCache { get; }

        protected LoggerMethodBase(DiscordSocketClient client, IConfiguration config, IMessageCache messageCache)
        {
            Client = client;
            Config = config;
            MessageCache = messageCache;
        }

        protected ISocketMessageChannel GetLoggerRoom()
        {
            var id = Convert.ToUInt64(Config["Discord:LoggerRoomID"]);
            var channel = Client.GetChannel(id);

            if (channel == null)
                throw new BotException($"Cannot find logger room with ID {id}");

            return (ISocketMessageChannel)channel;
        }
    }
}
