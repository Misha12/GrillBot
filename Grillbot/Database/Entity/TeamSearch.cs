using System;
using System.Collections.Generic;

namespace Grillbot.Database.Entity
{
    public partial class TeamSearch
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string ChannelId { get; set; }
        public string MessageId { get; set; }

        public ulong MessageIDSnowflake
        {
            get => Convert.ToUInt64(MessageId);
            set => MessageId = value.ToString();
        }

        public ulong ChannelIDSnowflake
        {
            get => Convert.ToUInt64(ChannelId);
            set => value.ToString();
        }
    }
}
