using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Grillbot.Services;
using Grillbot.Services.Config;
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

        public MessageDeletedHandler(DiscordSocketClient client, LoggerCache loggerCache, Statistics statistics, IConfiguration config)
        {
            Client = client;
            LoggerCache = loggerCache;
            Statistics = statistics;

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

            await Statistics.ChannelStats.DecrementCounterAsync(channel.Id);
            await LoggerCache.SendAttachmentToLoggerRoomAsync(message.Id);
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
