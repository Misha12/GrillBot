using Discord.WebSocket;

namespace Grillbot.Models.Internal
{
    public class CacheStatus
    {
        public SocketTextChannel Channel { get; set; }
        public int MessageCacheCount { get; set; }
        public int InternalCacheCount { get; set; }

        public CacheStatus(SocketTextChannel channel)
        {
            Channel = channel;
        }
    }
}
