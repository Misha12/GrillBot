using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Grillbot.Services;
using Grillbot.Services.Config;
using Grillbot.Services.Logger;
using Grillbot.Services.Statistics;
using Microsoft.Extensions.Configuration;

namespace Grillbot.Handlers
{
    public class MessageDeletedHandler : IConfigChangeable, IDisposable
    {
        private LoggerCache LoggerCache { get; }
        private Statistics Statistics { get; }
        private IConfiguration Config { get; set; }
        private DiscordSocketClient Client { get; }
        private Logger Logger { get; }

        public MessageDeletedHandler(DiscordSocketClient client, LoggerCache loggerCache, Statistics statistics, IConfiguration config, Logger logger)
        {
            Client = client;
            LoggerCache = loggerCache;
            Statistics = statistics;
            Logger = logger;

            ConfigChanged(config);

            Client.MessageDeleted += OnMessageDeletedAsync;
        }

        private async Task OnMessageDeletedAsync(Cacheable<IMessage, ulong> message, ISocketMessageChannel channel)
        {
            if(message.HasValue)
            {
                if(message.Value is SocketUserMessage userMessage)
                {
                    int argPos = 0;
                    if (userMessage.HasStringPrefix(Config["CommandPrefix"], ref argPos)) return;
                }

                if (message.Value.Author.IsBot || message.Value.Author.IsWebhook) return;
            }

            await Statistics.ChannelStats.DecrementCounterAsync(channel);
            await Logger.OnMessageDelete(message, channel);
        }

        public void ConfigChanged(IConfiguration newConfig)
        {
            Config = newConfig;
        }

        public void Dispose()
        {
            Client.MessageDeleted -= OnMessageDeletedAsync;
        }
    }
}
