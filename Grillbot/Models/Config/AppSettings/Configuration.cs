using System.Collections.Generic;

namespace Grillbot.Models.Config.AppSettings
{
    public class Configuration
    {
        public string AllowedHosts { get; set; }
        public string CommandPrefix { get; set; }
        public int EmoteChain_CheckLastCount { get; set; }
        public DiscordConfig Discord { get; set; }
        public string BackupErrors { get; set; }
    }
}
