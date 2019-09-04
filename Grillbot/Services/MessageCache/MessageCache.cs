using Discord;
using Discord.Net;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Services.MessageCache
{
    public class MessageCache : IMessageCache
    {
        private Dictionary<ulong, IMessage> Data { get; set; }
        private DiscordSocketClient Client { get; }

        public MessageCache(DiscordSocketClient client)
        {
            Data = new Dictionary<ulong, IMessage>();
            Client = client;
        }

        public async Task InitAsync()
        {
            if (Data.Count > 0)
                Data.Clear();

            var textChannels = Client.Guilds.SelectMany(o => o.Channels).Where(o => o is SocketTextChannel);

            foreach(var channel in textChannels.Select(o => (SocketTextChannel)o))
            {
                try
                {
                    var messages = (await channel.GetMessagesAsync().FlattenAsync()).ToList();
                    var pinnedMessages = await channel.GetPinnedMessagesAsync();

                    foreach (var message in messages)
                    {
                        Data.Add(message.Id, message);
                    }

                    foreach (var pinnedMessage in pinnedMessages)
                    {
                        if (!Exists(pinnedMessage.Id))
                            Data.Add(pinnedMessage.Id, pinnedMessage);
                    }
                }
                catch(HttpException httpEx)
                {
                    if (httpEx.DiscordCode != null && httpEx.DiscordCode.Value == 50001) continue;

                    throw;
                }
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

        public void Update(IMessage message)
        {
            if (!Exists(message.Id)) return;

            Data[message.Id] = message;
        }
    }
}
