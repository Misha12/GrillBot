using Discord;
using Discord.WebSocket;
using Grillbot.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Grillbot.Database.Entity
{
    [Table("AutoReply")]
    public class AutoReplyItem
    {
        [Key]
        [Column]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Column]
        [Required]
        public string MustContains { get; set; }

        [Column]
        [Required]
        public string ReplyMessage { get; set; }

        [NotMapped]
        public int CallsCount { get; set; }

        [Column]
        public AutoReplyCompareTypes CompareType { get; set; }

        [Column]
        [Required]
        public string GuildID { get; set; }

        [NotMapped]
        public ulong GuildIDSnowflake
        {
            get => Convert.ToUInt64(GuildID);
            set => GuildID = value.ToString();
        }

        [Column]
        public string ChannelID { get; set; }

        [NotMapped]
        public ulong? ChannelIDSnowflake
        {
            get => string.IsNullOrEmpty(ChannelID) ? (ulong?)null : Convert.ToUInt64(ChannelID);
            set => ChannelID = value?.ToString();
        }

        public int Flags { get; set; }

        [NotMapped]
        public StringComparison StringComparison => (Flags & (int)AutoReplyParams.CaseSensitive) != 0 ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase;
        public bool IsDisabled => (Flags & (int)AutoReplyParams.Disabled) != 0;

        public bool CanReply(SocketGuild guild, IChannel channel)
        {
            if (IsDisabled || guild.Id != GuildIDSnowflake)
                return false;

            return ChannelIDSnowflake == null || channel.Id == ChannelIDSnowflake;
        }
    }
}
