using System.Collections.Generic;

namespace Grillbot.Models.BotStatus
{
    public class WebStatus
    {
        public SimpleBotStatus Simple { get; set; }
        public Dictionary<string, ulong> ExecutedCommands { get; set; }
        public Dictionary<string, uint> LoggerStats { get; set; }
        public Dictionary<string, int> DBStatus { get; set; }
        public Dictionary<string, ulong> TriggeredEvents { get; set; }
    }
}
