using System;

namespace Grillbot.Models.BotStatus
{
    public class SimpleBotStatus
    {
        public string RamUsage { get; set; }
        public DateTime StartTime { get; set; }
        public string ThreadStatus { get; set; }
        public string InstanceType { get; set; }
        public int ActiveWebTokensCount { get; set; }
        public TimeSpan AvgReactTime { get; set; }
        public TimeSpan ActiveCpuTime { get; set; }
    }
}
