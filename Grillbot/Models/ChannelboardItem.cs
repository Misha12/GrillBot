using Grillbot.Extensions;
using System;

namespace Grillbot.Models
{
    public class ChannelboardItem
    {
        public string ChannelName { get; set; }
        public long Count { get; set; }
        public DateTime LastMessageAt { get; set; }
    }
}
