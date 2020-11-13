namespace Grillbot.Models.Config.AppSettings
{
    public class Configuration
    {
        public string AllowedHosts { get; set; }
        public DiscordConfig Discord { get; set; }
        public string BackupErrors { get; set; }
    }
}
