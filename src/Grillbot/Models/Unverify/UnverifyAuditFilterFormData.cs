using Discord.WebSocket;
using Grillbot.Database.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Grillbot.Models.Unverify
{
    public class UnverifyAuditFilterFormData
    {
        [Required]
        public ulong GuildID { get; set; }
        public string FromUserQuery { get; set; }
        public string ToUserQuery { get; set; }
        public UnverifyLogOperation? Operation { get; set; }
        public DateTime? DateTimeFrom { get; set; }
        public DateTime? DateTimeTo { get; set; }
        public bool OrderAsc { get; set; }

        [Range(0, double.MaxValue)]
        public int Page { get; set; } = 1;

        public UnverifyAuditFilterFormData() { }

        public UnverifyAuditFilterFormData(DiscordSocketClient client)
        {
            GuildID = client.Guilds.FirstOrDefault()?.Id ?? 0;
        }
    }
}
