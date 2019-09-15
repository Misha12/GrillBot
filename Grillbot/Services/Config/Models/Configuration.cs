namespace Grillbot.Services.Config.Models
{
    public class Configuration
    {
        public string AllowedHosts { get; set; }
        public string CommandPrefix { get; set; }
        public string Database { get; set; }
        public int EmoteChain_CheckLastCount { get; set; }
        public DiscordConfig Discord { get; set; }
        public BotLogConfig Log { get; set; }
        public MethodsConfig MethodsConfig { get; set; }
    }
}
