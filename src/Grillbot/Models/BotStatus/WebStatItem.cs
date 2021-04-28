using Discord;

namespace Grillbot.Models.BotStatus
{
    public class WebStatItem
    {
        public long Id { get; set; }

        public IGuild Guild { get; set; }
        public IGuildUser User { get; set; }

        public int? WebAdminLoginCount { get; set; }
        public bool HaveWebAdminAccess { get; set; }

        public WebStatItem(IGuild guild, IGuildUser user, Database.Entity.Users.Reporting.WebStatItem entity)
        {
            Id = entity.Id;
            Guild = guild;
            User = user;
            WebAdminLoginCount = entity.WebAdminLoginCount;
            HaveWebAdminAccess = entity.HaveWebAdminAccess;
        }
    }
}
