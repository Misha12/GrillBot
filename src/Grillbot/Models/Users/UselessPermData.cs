using Discord.WebSocket;

namespace Grillbot.Models.Users
{
    public class UselessPermData
    {
        public SocketGuildUser User { get; set; }
        public bool NeutralUseless { get; set; }
        public bool AllowDenyUseless { get; set; }

        public UselessPermData(SocketGuildUser user, bool neutralUseless, bool allowDenyUseless)
        {
            User = user;
            NeutralUseless = neutralUseless;
            AllowDenyUseless = allowDenyUseless;
        }
    }
}
