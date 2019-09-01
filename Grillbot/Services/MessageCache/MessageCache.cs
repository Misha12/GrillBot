using Discord;
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
            var textChannels = Client.Guilds.SelectMany(o => o.Channels).Where(o => o is SocketTextChannel);

            foreach(var channel in textChannels.Select(o => (SocketTextChannel)o))
            {
                var messages = (await channel.GetMessagesAsync().FlattenAsync()).ToList();
                var pinnedMessages = await channel.GetPinnedMessagesAsync();

                foreach(var message in messages)
                {
                    Data.Add(message.Id, message);
                }

                foreach(var pinnedMessage in pinnedMessages)
                {
                    if (!Exists(pinnedMessage.Id))
                        Data.Add(pinnedMessage.Id, pinnedMessage);
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
