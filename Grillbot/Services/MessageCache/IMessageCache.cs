using Discord;
using System;
using System.Threading.Tasks;

namespace Grillbot.Services.MessageCache
{
    public interface IMessageCache : IDisposable
    {
        Task InitAsync();
        IMessage TryRemove(ulong id);
        IMessage Get(ulong id);
        void Update(IMessage message);
        bool Exists(ulong id);
        Task<IMessage> GetAsync(ulong channelID, ulong messageID);
    }
}
