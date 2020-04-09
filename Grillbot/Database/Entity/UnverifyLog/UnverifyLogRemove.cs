using System.Collections.Generic;

namespace Grillbot.Database.Entity.UnverifyLog
{
    public class UnverifyLogRemove : UnverifyLogDataBase
    {
        public List<ulong> Roles { get; set; }
        public List<ChannelOverride> Overrides { get; set; }
    }
}
