using System.Collections.Generic;

namespace Grillbot.Services.Config.Models
{
    public class MemesConfig : MethodConfigBase
    {
        public List<string> AllowedChannels { get; set; }
        public string UnverifyTime { get; set; }
        public string WherePointsChannelID { get; set; }
        public string UnverifyReason { get; set; }

        public MemesConfig()
        {
            AllowedChannels = new List<string>();
        }
    }
}
