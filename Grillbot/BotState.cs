using System;
using System.Collections.Generic;

namespace Grillbot
{
    public class BotState
    {
        public Dictionary<string, DateTime> LastPointsCalculation { get; set; }

        public BotState()
        {
            LastPointsCalculation = new Dictionary<string, DateTime>();
        }
    }
}
