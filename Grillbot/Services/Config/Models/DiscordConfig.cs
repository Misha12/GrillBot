using Discord.WebSocket;
using System;
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
        public string LoggerRoomID { get; set; }
        public string ServerBoosterRoleId { get; set; }

        public ulong ServerBoosterRoleIdSnowflake
        {
            get => Convert.ToUInt64(ServerBoosterRoleId);
            set => ServerBoosterRoleId = value.ToString();
        }

        public bool IsBooster(IReadOnlyCollection<SocketRole> roles) => roles.Any(o => o.Id == ServerBoosterRoleIdSnowflake);
    }
}
