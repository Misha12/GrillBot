using Discord;
using Grillbot.Services.Statistics.ApiStats;
using System;
using System.Collections.Generic;

namespace Grillbot.Models.BotStatus
{
    public class ReportsViewModel
    {
        public Dictionary<string, ulong> Commands { get; set; }
        public Dictionary<string, ulong> Events { get; set; }
        public Dictionary<string, Tuple<int, long>> Database { get; set; }
        public GCMemoryInfo GCMemoryInfo { get; set; }
        public SimpleBotStatus BotStatus { get; set; }
        public List<ApiStatsData> Api { get; set; }
        public ConnectionState ConnectionState { get; set; }
        public int Latency { get; set; }
        public LoginState LoginState { get; set; }
    }
}
