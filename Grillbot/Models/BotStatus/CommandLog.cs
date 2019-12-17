using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Models.BotStatus
{
    public class CommandLog
    {
        public long ID { get; set; }
        public string Group { get; set; }
        public string Command { get; set; }
        public string Username { get; set; }
        public DateTime CalledAt { get; set; }
        public string GuildName { get; set; }
        public string ChannelName { get; set; }
        public string FullCommand { get; set; }
    }
}
