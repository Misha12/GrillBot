namespace Grillbot.Models.Config.AppSettings
{
    public class DiscordConfig
    {
        public string Token { get; set; }
        public string UserJoinedMessage { get; set; }
        public ulong? ErrorLogChannelID { get; set; }
    }
}
