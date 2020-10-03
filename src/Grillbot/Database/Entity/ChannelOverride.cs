using Discord;
using Newtonsoft.Json;
using System;

namespace Grillbot.Database.Entity
{
    public class ChannelOverride
    {
        public string ChannelId { get; set; }

        [JsonIgnore]
        public ulong ChannelIdSnowflake
        {
            get => Convert.ToUInt64(ChannelId);
            set => ChannelId = value.ToString();
        }

        public ulong AllowValue { get; set; }
        public ulong DenyValue { get; set; }

        public OverwritePermissions GetPermissions() => new OverwritePermissions(AllowValue, DenyValue);

        public ChannelOverride() { }

        public ChannelOverride(ulong channelId, OverwritePermissions permissions)
        {
            ChannelIdSnowflake = channelId;
            AllowValue = permissions.AllowValue;
            DenyValue = permissions.DenyValue;
        }
    }
}
