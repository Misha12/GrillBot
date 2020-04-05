using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Models.TeamSearch
{
    public class TeamSearchItem
    {
        public int ID { get; set; }
        public string GuildName { get; set; }
        public string ChannelName { get; set; }
        public string ShortUsername { get; set; }
        public string FullUsername { get; set; }
        public string Message { get; set; }
        public string MessageLink { get; set; }
    }
}
