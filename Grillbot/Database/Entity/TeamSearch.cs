using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Grillbot.Database.Entity
{
    public partial class TeamSearch
    {
        public int Id { get; set; }

        [StringLength(30)]
        public string UserId { get; set; }

        [StringLength(30)]
        public string ChannelId { get; set; }

        [StringLength(30)]
        public string MessageId { get; set; }

        [StringLength(30)]
        public string GuildId { get; set; }

        [NotMapped]
        public ulong UserIDSnowflake
        {
            get => Convert.ToUInt64(UserId);
            set => UserId = value.ToString();
        }

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
            set => ChannelId = value.ToString();
        }

        [NotMapped]
        public ulong GuildIDSnowflake
        {
            get => Convert.ToUInt64(GuildId);
            set => GuildId = value.ToString();
        }
    }
}
