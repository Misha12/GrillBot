using Discord;
using System;
using System.Collections.Generic;

namespace Grillbot.Models.BotStatus
{
    public class ReportsViewModel
    {
        public Dictionary<string, Tuple<ulong, int>> Commands { get; set; }
        public Dictionary<string, ulong> Events { get; set; }
        public Dictionary<string, Tuple<int, long>> Database { get; set; }
        public GCMemoryInfo GCMemoryInfo { get; set; }
        public SimpleBotStatus BotStatus { get; set; }
        public ConnectionState ConnectionState { get; set; }
        public int Latency { get; set; }
        public LoginState LoginState { get; set; }
        public List<BackgroundTaskQueueGroup> BackgroundTasks { get; set; }
    }
}
