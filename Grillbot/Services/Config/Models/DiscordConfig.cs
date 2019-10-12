namespace Grillbot.Services.Config.Models
{
    public class DiscordConfig
    {
        public string Activity { get; set; }

        [StrictPrivate]
        public string Token { get; set; }

        [StrictPrivate]
        public ulong ClientId { get; set; }

        [StrictPrivate]
        public string ClientSecret { get; set; }

        public string UserJoinedMessage { get; set; }
        public string LoggerRoomID { get; set; }
    }
}
