using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;

namespace Grillbot.Models.Config.AppSettings
{
    public class DiscordConfig
    {
        public string Activity { get; set; }
        public string Token { get; set; }
        public ulong ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string UserJoinedMessage { get; set; }
        public ulong? LoggerRoomID { get; set; }
        public ulong? ServerBoosterRoleId { get; set; }
        public ulong? AdminChannelID { get; set; }
        public ulong? ErrorLogChannelID { get; set; }

        public bool IsBooster(IReadOnlyCollection<SocketRole> roles)
        {
            return roles.Any(o => o.Id == ServerBoosterRoleId);
        }
    }
}
