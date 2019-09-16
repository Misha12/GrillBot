using System.Collections.Generic;
using System.Linq;

namespace Grillbot.Services.Config.Models
{
    public class Configuration
    {
        public string AllowedHosts { get; set; }
        public string CommandPrefix { get; set; }

        [StrictPrivate]
        public string Database { get; set; }

        public int EmoteChain_CheckLastCount { get; set; }
        public DiscordConfig Discord { get; set; }
        public BotLogConfig Log { get; set; }
        public MethodsConfig MethodsConfig { get; set; }

        public List<string> Administrators { get; set; }

        public Configuration()
        {
            Administrators = new List<string>();
        }

        public bool IsUserBotAdmin(ulong id) => Administrators.Any(o => o == id.ToString());
    }
}
