using Discord;
using Discord.Net;
using Discord.WebSocket;
using Grillbot.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Grillbot.Services.MessageCache
{
    public class MessageCache : IMessageCache
    {
        private ConcurrentDictionary<ulong, IMessage> Data { get; set; }
        private DiscordSocketClient Client { get; }
        private ILogger<MessageCache> Logger { get; }

        public MessageCache(DiscordSocketClient client, ILogger<MessageCache> logger)
        {
            Data = new ConcurrentDictionary<ulong, IMessage>();
            Client = client;
            Logger = logger;
        }

        public async Task InitAsync()
        {
            var options = new RequestOptions() { RetryMode = RetryMode.RetryRatelimit | RetryMode.RetryTimeouts, Timeout = 5000 };

            foreach (var channel in Client.Guilds.SelectMany(o => o.TextChannels))
            {
                await InitChannel(Data, channel, options);
            }
        }

        private async Task InitChannel(ConcurrentDictionary<ulong, IMessage> messages, SocketTextChannel channel, RequestOptions options = null)
        {
            try
            {
                var messagesFromApi = await channel.GetMessagesAsync(options: options).FlattenAsync();

                foreach (var message in messagesFromApi)
                {
                    messages.TryAdd(message.Id, message);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Cannot load channel {channel.Name} ({channel.Id}) ({channel.Guild?.Name ?? "NoGuild"}) to cache.");
            }
        }

        public async Task AppendAroundAsync(IMessageChannel channel, ulong messageID, int limit = 50)
        {
            try
            {
                var messages = await channel.GetMessagesAsync(messageID, Direction.Around, limit).FlattenAsync();

                foreach (var message in messages)
                {
                    Data.TryAdd(message.Id, message);
                }
            }
            catch (HttpException ex) when (ex.HttpCode == HttpStatusCode.InternalServerError) { /* Internal server error can ignore. */ }
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

            if (Client.GetChannel(channelID) is not ISocketMessageChannel channel)
                return null;

            var message = await channel.GetMessageAsync(messageID).ConfigureAwait(false);

            if (message == null)
                return null;

            Data.TryAdd(message.Id, message);
            return message;
        }

        public void Update(IMessage message)
        {
            if (!Exists(message.Id)) return;

            var oldValue = Get(message.Id);
            Data.TryUpdate(message.Id, message, oldValue);
        }

        public void Init() { }

        public List<IMessage> TryBulkDelete(IEnumerable<ulong> messageIds)
        {
            return messageIds
                .Select(id => TryRemove(id))
                .Where(o => o != null)
                .ToList();
        }

        public IEnumerable<IMessage> GetFromChannel(ulong channelId)
        {
            return Data.Values
                .Where(o => o.Channel != null && o.Channel.Id == channelId);
        }
    }
}
