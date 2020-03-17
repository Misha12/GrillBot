using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Grillbot.Database.Entity
{
    public partial class TeamSearch
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string ChannelId { get; set; }
        public string MessageId { get; set; }

        [NotMapped]
        public ulong MessageIDSnowflake
        {
            get => Convert.ToUInt64(MessageId);
            set => MessageId = value.ToString();
        }

        [NotMapped]
        public ulong ChannelIDSnowflake
        {
            get => Convert.ToUInt64(ChannelId);
            set => value.ToString();
        }
    }
}
