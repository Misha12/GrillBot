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

        public ulong ChannelID { get; set; }
        public ulong AllowValue { get; set; }
        public ulong DenyValue { get; set; }

        #endregion

        public ChannelOverwrite() { }

        public ChannelOverwrite(IChannel channel, OverwritePermissions? perms)
        {
            Channel = channel;

            if (channel != null)
                ChannelID = channel.Id;

            Permissions = perms;

            if (perms != null)
            {
                AllowValue = Perms.AllowValue;
                DenyValue = Perms.DenyValue;
            }
        }
    }
}
