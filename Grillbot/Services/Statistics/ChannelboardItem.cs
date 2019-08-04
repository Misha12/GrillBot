using System;

namespace Grillbot.Services.Statistics
{
    public class ChannelboardItem
    {
        public string ChannelName { get; set; }
        public long Count { get; set; }
        public DateTime LastMessageAt { get; set; }
    }
}
