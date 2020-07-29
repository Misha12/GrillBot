using Grillbot.Services.InviteTracker;
using System;
using System.Collections.Generic;

namespace Grillbot
{
    public class BotState
    {
        public Dictionary<string, DateTime> LastPointsCalculation { get; set; }
        public Dictionary<ulong, List<InviteModel>> InviteCache { get; set; }

        public BotState()
        {
            LastPointsCalculation = new Dictionary<string, DateTime>();
            InviteCache = new Dictionary<ulong, List<InviteModel>>();
        }
    }
}
