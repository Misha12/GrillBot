using System.Collections.Generic;
using System.Linq;

namespace Grillbot.Models.Config.AppSettings
{
    public class Configuration
    {
        public string AllowedHosts { get; set; }
        public string CommandPrefix { get; set; }
        public string Database { get; set; }
        public int EmoteChain_CheckLastCount { get; set; }
        public DiscordConfig Discord { get; set; }

        public List<string> Administrators { get; set; }

        public Configuration()
        {
            Administrators = new List<string>();
        }

        public bool IsUserBotAdmin(ulong id)
        {
            return Administrators.Any(o => o == id.ToString());
        }
    }
}
