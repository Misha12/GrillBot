using Discord;
using Grillbot.Services.Initiable;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grillbot.Services.MessageCache
{
    public interface IMessageCache : IDisposable, IInitiable
    {
        IMessage TryRemove(ulong id);
        List<IMessage> TryBulkDelete(IEnumerable<ulong> messageIds);
        IMessage Get(ulong id);
        void Update(IMessage message);
        bool Exists(ulong id);
        Task<IMessage> GetAsync(ulong channelID, ulong messageID);
    }
}
