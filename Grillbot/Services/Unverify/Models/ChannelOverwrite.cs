using Discord;
using Newtonsoft.Json;

namespace Grillbot.Services.Unverify.Models
{
    public class ChannelOverwrite
    {
        public IChannel Channel { get; set; }
        public OverwritePermissions? Permissions { get; set; }

        [JsonIgnore]
        public OverwritePermissions Perms => Permissions.Value;
    }
}
