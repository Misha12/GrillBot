using System.Collections.Generic;

namespace Grillbot.Services.Unverify.Models.Log
{
    public class UnverifyLogRemove : UnverifyLogBase
    {
        public List<ulong> ReturnedRoles { get; set; }
        public List<ChannelOverwrite> ReturnedOverrides { get; set; }
    }
}
