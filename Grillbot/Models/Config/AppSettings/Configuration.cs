using System.Collections.Generic;

namespace Grillbot.Models.Config.AppSettings
{
    public class Configuration
    {
        public string AllowedHosts { get; set; }
        public string CommandPrefix { get; set; }
        public string Database { get; set; }
        public int EmoteChain_CheckLastCount { get; set; }
        public DiscordConfig Discord { get; set; }
        public string PeepoloveDir { get; set; }

        public List<ulong> Administrators { get; set; }

        public Configuration()
        {
            Administrators = new List<ulong>();
        }

        public bool IsUserBotAdmin(ulong id)
        {
            return Administrators.Contains(id);
        }
    }
}
