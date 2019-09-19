using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grillbot.Services.MessageCache
{
    public class MessageCache : IMessageCache
    {
        private Dictionary<ulong, IMessage> Data { get; set; }
        private DiscordSocketClient Client { get; }
        private BotLoggingService LoggingService { get; }

        public MessageCache(DiscordSocketClient client, BotLoggingService loggingService)
        {
            Data = new Dictionary<ulong, IMessage>();
            Client = client;
            LoggingService = loggingService;
        }

        public async Task InitAsync()
        {
            if (Data.Count > 0)
                Data.Clear();

            var textChannels = Client.Guilds.SelectMany(o => o.Channels).OfType<SocketTextChannel>().ToList();
            var options = new RequestOptions() { RetryMode = RetryMode.RetryRatelimit | RetryMode.RetryTimeouts, Timeout = 15000 };

            foreach (var channel in textChannels)
            {
                await InitChannel(channel, options);
                Thread.Sleep(900);
            }
        }

        private async Task InitChannel(SocketTextChannel channel, RequestOptions options = null, bool removeExists = false)
        {
            try
            {
                var messages = (await channel.GetMessagesAsync(50, options).FlattenAsync()).ToList();

                foreach (var message in messages)
                {
                    if (removeExists && Data.ContainsKey(message.Id))
                        Data.Remove(message.Id);

                    Data.Add(message.Id, message);
                }
            }
            catch (Exception ex)
            {
                LoggingService.WriteToLog($"Cannot load channel {channel.Name} ({channel.Id}) to cache. {ex}");
            }
        }

        public IMessage TryRemove(ulong id)
        {
            return Data.Remove(id, out IMessage message) ? message : null;
        }

        public bool Exists(ulong id)
        {
            return Data.ContainsKey(id);
        }

        public void Dispose()
        {
            Data.Clear();
            Data = null;
        }

        public IMessage Get(ulong id)
        {
            return Data.TryGetValue(id, out IMessage message) ? message : null;
        }

        public async Task<IMessage> GetAsync(ulong channelID, ulong messageID)
        {
            if (Exists(messageID))
                return Get(messageID);

            if (!(Client.GetChannel(channelID) is ISocketMessageChannel channel))
                return null;

            var message = await channel.GetMessageAsync(messageID);
            Data.Add(message.Id, message);

            return message;
        }

        public void Update(IMessage message)
        {
            if (!Exists(message.Id)) return;

            Data[message.Id] = message;
        }
    }
}
