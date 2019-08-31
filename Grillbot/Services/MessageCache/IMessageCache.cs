using Discord;
using System;
using System.Threading.Tasks;

namespace Grillbot.Services.MessageCache
{
    public interface IMessageCache : IDisposable
    {
        Task InitAsync();
        IMessage TryRemove(ulong id);
        bool Exists(ulong id);
    }
}
