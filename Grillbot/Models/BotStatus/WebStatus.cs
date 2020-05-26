using Discord;
using System;
using System.Collections.Generic;

namespace Grillbot.Models.BotStatus
{
    public class WebStatus
    {
        public SimpleBotStatus Simple { get; set; }
        public Dictionary<string, ulong> ExecutedCommands { get; set; }
        public Dictionary<string, uint> LoggerStats { get; set; }
        public Dictionary<string, Tuple<int, long>> DBStatus { get; set; }
        public Dictionary<string, ulong> TriggeredEvents { get; set; }
        public int Latency { get; set; }
        public ConnectionState ConnectionState { get; set; }
        public LoginState LoginState { get; set; }
    }
}
