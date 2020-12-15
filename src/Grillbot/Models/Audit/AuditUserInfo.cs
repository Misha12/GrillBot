using Discord;
using Newtonsoft.Json;

namespace Grillbot.Models.Audit
{
    public class AuditUserInfo
    {
        [JsonProperty("id")]
        public ulong Id { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("tag_id")]
        public string Discriminator { get; set; }

        public AuditUserInfo() { }

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
