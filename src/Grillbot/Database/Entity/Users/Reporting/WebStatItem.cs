using System;

namespace Grillbot.Database.Entity.Users.Reporting
{
    public class WebStatItem
    {
        public long Id { get; set; }
        public string GuildId { get; set; }

        public ulong GuildIdSnowflake
        {
            get => Convert.ToUInt64(GuildId);
            set => GuildId = value.ToString();
        }

        public string UserId { get; set; }

        public ulong UserIdSnowflake
        {
            get => Convert.ToUInt64(UserId);
            set => UserId = value.ToString();
        }

        public int? WebAdminLoginCount { get; set; }
        public bool HaveWebAdminAccess { get; set; }
    }
}
