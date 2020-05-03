using System;
using System.Collections.Generic;

namespace Grillbot.Database.Entity.UnverifyLog
{
    public class UnverifyLogSet
    {
        public string TimeFor { get; set; }
        public DateTime StartAt { get; set; }
        public List<ulong> Roles { get; set; }
        public List<ChannelOverride> Overrides { get; set; }
        public string Reason { get; set; }
        public bool IsSelfUnverify { get; set; }
        public List<string> Subjects { get; set; }
    }
}
