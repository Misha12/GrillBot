using System;

namespace Grillbot.Services.Config.Models
{
    public class ChannelboardConfig
    {
        public int WebTokenValidMinutes { get; set; }
        public string WebUrl { get; set; }

        public TimeSpan GetTokenValidTime() => TimeSpan.FromMinutes(WebTokenValidMinutes);
    }
}
