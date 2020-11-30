using Grillbot.Models;
using System;

namespace Grillbot.Services.Unverify
{
    public class UnverifyBackgroundTask : BackgroundTask<UnverifyService>
    {
        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
        public DateTime At { get; set; }

        public override bool CanProcess()
        {
            return (At - DateTime.Now).TotalSeconds <= 0.0F;
        }

        public UnverifyBackgroundTask(ulong guildId, ulong userId, DateTime at)
        {
            GuildId = guildId;
            UserId = userId;
            At = at;
        }
    }
}
