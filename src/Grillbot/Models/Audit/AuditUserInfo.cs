using Discord;

namespace Grillbot.Models.Audit
{
    public class AuditUserInfo
    {
        public ulong Id { get; set; }
        public string Username { get; set; }
        public string Discriminator { get; set; }

        public static AuditUserInfo Create(IUser user)
        {
            return new AuditUserInfo()
            {
                Discriminator = user.Discriminator,
                Id = user.Id,
                Username = user.Username
            };
        }
    }
}
