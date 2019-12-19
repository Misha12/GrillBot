using System;
using System.Collections.Generic;

namespace Grillbot.Repository.Entity.UnverifyLog
{
    public class UnverifyLogSet : UnverifyLogDataBase
    {
        public string TimeFor { get; set; }
        public DateTime StartAt { get; set; }
        public List<string> Roles { get; set; }
        public List<ChannelOverride> Overrides { get; set; }
        public string Reason { get; set; }
    }
}
