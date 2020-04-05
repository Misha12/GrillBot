using Grillbot.Services.Statistics;
using System.Collections.Generic;

namespace Grillbot.Models
{
    public class ChannelboardViewModel
    {
        public List<ChannelboardItem> Items { get; set; }
        public GuildInfo Guild { get; set; }
        public GuildUser User { get; set; }
        public ChannelboardErrors Error { get; set; }

        public ChannelboardViewModel()
        {
            Items = new List<ChannelboardItem>();
            Guild = new GuildInfo();
        }
    }
}
