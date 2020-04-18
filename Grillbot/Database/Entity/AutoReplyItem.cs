using Discord;
using Discord.WebSocket;
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

        [Column]
        [Required]
        public bool IsDisabled { get; set; }

        [NotMapped]
        public int CallsCount { get; set; }

        public bool CanReply(SocketGuild guild, IChannel channel)
        {
            bool validGuild = guild.Id == GuildIDSnowflake;
            bool haveReply = !string.IsNullOrEmpty(ReplyMessage);
            bool haveChannel = ChannelIDSnowflake == null || channel.Id == ChannelIDSnowflake;

            return validGuild && !IsDisabled && haveReply && haveChannel;
        }

        [Column]
        public AutoReplyCompareTypes CompareType { get; set; }

        [Column]
        public bool CaseSensitive { get; set; }

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

        public void SetCompareType(string type)
        {
            switch(type.ToLower())
            {
                case "absolute":
                case "==":
                    CompareType = AutoReplyCompareTypes.Absolute;
                    break;
                case "contains":
                    CompareType = AutoReplyCompareTypes.Contains;
                    break;
            }
        }
    }
}
