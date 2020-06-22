using System;
using System.Collections.Generic;

namespace Grillbot.Models.TempUnverify
{
    public class CurrentUnverifiedUser
    {
        public int ID { get; set; }
        public string Username { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime EndDateTime { get; set; }
        public List<string> Roles { get; set; }
        public string ChannelOverrideList { get; set; }
        public string Reason { get; set; }
        public string GuildName { get; set; }
        public bool IsSelfUnverify { get; set; }
    }
}
