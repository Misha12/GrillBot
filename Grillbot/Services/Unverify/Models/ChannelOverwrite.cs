using Discord;
using Newtonsoft.Json;

namespace Grillbot.Services.Unverify.Models
{
    public class ChannelOverwrite
    {
        [JsonIgnore]
        public IChannel Channel { get; set; }

        [JsonIgnore]
        public OverwritePermissions? Permissions { get; set; }

        [JsonIgnore]
        public OverwritePermissions Perms => Permissions.Value;

        #region JSON fields

        public ulong ChannelID => Channel.Id;
        public ulong AllowValue => Perms.AllowValue;
        public ulong DenyValue => Perms.DenyValue;

        #endregion
    }
}
