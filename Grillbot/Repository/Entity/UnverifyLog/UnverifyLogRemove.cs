using System.Collections.Generic;

namespace Grillbot.Repository.Entity.UnverifyLog
{
    public class UnverifyLogRemove : UnverifyLogDataBase
    {
        public List<string> Roles { get; set; }
        public List<ChannelOverride> Overrides { get; set; }
    }
}
