using System.Collections.Generic;

namespace Grillbot.Models.CallStats
{
    public class CallStatsViewModel
    {
        public List<CommandStatSummaryItem> Commands { get; set; }

        public CallStatsViewModel(List<CommandStatSummaryItem> commands)
        {
            Commands = commands;
        }
    }
}
