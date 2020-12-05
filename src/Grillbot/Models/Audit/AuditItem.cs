using Discord.WebSocket;
using Grillbot.Database.Entity.AuditLog;
using Grillbot.Enums;
using Grillbot.Models.Users;
using Newtonsoft.Json.Linq;
using System;

namespace Grillbot.Models.Audit
{
    public class AuditItem
    {
        public long Id { get; set; }
        public DiscordUser User { get; set; }
        public DateTime CreatedAt { get; set; }
        public SocketGuild Guild { get; set; }
        public AuditLogType Type { get; set; }

        private JObject JsonData { get; }

        public AuditItem() { }
        public AuditItem(JObject jsonData)
        {
            JsonData = jsonData;
        }

        #region Typed properties

        public CommandAuditData CommandAuditData => Type == AuditLogType.Command ? JsonData.ToObject<CommandAuditData>().GetFilledModel(Guild) : null;
        public UserLeftAuditData UserLeftAuditData => Type == AuditLogType.UserLeft ? JsonData.ToObject<UserLeftAuditData>() : null;

        #endregion

        public static AuditItem Create(SocketGuild guild, AuditLogItem dbItem, DiscordUser user)
        {
            return new AuditItem(dbItem.Data)
            {
                CreatedAt = dbItem.CreatedAt,
                Guild = guild,
                Id = dbItem.Id,
                Type = dbItem.Type,
                User = user
            };
        }
    }
}
