using System.Collections.Generic;
using System.Linq;

namespace Grillbot.Services.Config.Models
{
    public class DiscordConfig
    {
        public string Activity { get; set; }

        [StrictPrivate]
        public string Token { get; set; }

        public string UserJoinedMessage { get; set; }
        public List<string> Administrators { get; set; }
        public string LoggerRoomID { get; set; }

        public DiscordConfig()
        {
            Administrators = new List<string>();
        }

        public bool IsUserBotAdmin(ulong id) => Administrators.Any(o => o == id.ToString());
    }
}
