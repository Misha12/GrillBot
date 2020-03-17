using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Models.BotStatus
{
    public class WebStatus
    {
        public SimpleBotStatus Simple { get; set; }
        public List<StatisticsData> CallStats { get; set; }
        public Dictionary<string, uint> LoggerStats { get; set; }
        public Dictionary<string, int> DBStatus { get; set; }
        public Dictionary<string, string> CalledEventStats { get; set; }
    }
}
