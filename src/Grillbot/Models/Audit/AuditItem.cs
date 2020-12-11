using Discord.WebSocket;
using Grillbot.Database.Entity.AuditLog;
using Grillbot.Enums;
using Grillbot.Models.Audit.DiscordAuditLog;
using Grillbot.Models.Users;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grillbot.Models.Audit
{
    public class AuditItem
    {
        public long Id { get; set; }
        public DiscordUser User { get; set; }
        public DateTime CreatedAt { get; set; }
        public SocketGuild Guild { get; set; }
        public AuditLogType Type { get; set; }

        private string JsonData { get; }

        public List<string> AttachmentNames { get; set; }

        public AuditItem() { }
        public AuditItem(string jsonData)
        {
            JsonData = jsonData;
        }

        #region Typed properties

        public CommandAuditData CommandAuditData => Type == AuditLogType.Command ? JsonConvert.DeserializeObject<CommandAuditData>(JsonData).GetFilledModel(Guild) : null;
        public UserLeftAuditData UserLeftAuditData => Type == AuditLogType.UserLeft ? JsonConvert.DeserializeObject<UserLeftAuditData>(JsonData) : null;
        public UserJoinedAuditData UserJoinedAuditData => Type == AuditLogType.UserJoined ? JsonConvert.DeserializeObject<UserJoinedAuditData>(JsonData).GetFilledModel(User) : null;
        public MessageEditedAuditData MessageEditedAuditData => Type == AuditLogType.MessageEdited ? JsonConvert.DeserializeObject<MessageEditedAuditData>(JsonData).GetFilledModel(Guild) : null;
        public MessageDeletedAuditData MessageDeletedAuditData => Type == AuditLogType.MessageDeleted ? JsonConvert.DeserializeObject<MessageDeletedAuditData>(JsonData).GetFilledModel(Guild) : null;
        public AuditBotAdded BotAdded => AuditBotAdded.FromJsonIfValid(Type, JsonData);
        public AuditChannelInfo ChannelInfo => AuditChannelInfo.FromJsonIfValid(Type, JsonData);
        public GuildUpdated GuildUpdated => GuildUpdated.FromJsonIfValid(Type, JsonData)?.GetFilledModel(Guild);
        public AuditChannelUpdated ChannelUpdated => AuditChannelUpdated.FromJsonIfValid(Type, JsonData);
        public AuditEmoteInfo EmoteInfo => AuditEmoteInfo.FromJsonIfValid(Type, JsonData);
        public AuditEmoteUpdated EmoteUpdated => AuditEmoteUpdated.FromJsonIfValid(Type, JsonData);
        public AuditOverwriteInfo OverwriteInfo => AuditOverwriteInfo.FromJsonIfValid(Type, JsonData)?.GetFilledModel(Guild);
        public AuditOverwriteUpdated OverwriteUpdated => AuditOverwriteUpdated.FromJsonIfValid(Type, JsonData)?.GetFilledModel(Guild);
        public AuditPruneMembers PruneMembers => AuditPruneMembers.FromJsonIfValid(Type, JsonData);
        public AuditUnban Unban => AuditUnban.FromJsonIfValid(Type, JsonData);
        public AuditMemberUpdated MemberUpdated => AuditMemberUpdated.FromJsonIfValid(Type, JsonData)?.GetFilledModel(Guild);
        public Role Role => Role.FromJsonIfValid(Type, JsonData);
        public RoleUpdated RoleUpdated => RoleUpdated.FromJsonIfValid(Type, JsonData);
        public Webhook Webhook => Webhook.FromJsonIfValid(Type, JsonData);
        public WebhookUpdated WebhookUpdated => WebhookUpdated.FromJsonIfValid(Type, JsonData)?.GetFilledModel(Guild);
        public AuditMessagePinInfo PinInfo => AuditMessagePinInfo.FromJsonIfValid(Type, JsonData).GetFilledModel(Guild);

        #endregion

        public static AuditItem Create(SocketGuild guild, AuditLogItem dbItem, DiscordUser user)
        {
            return new AuditItem(dbItem.JsonData)
            {
                CreatedAt = dbItem.CreatedAt,
                Guild = guild,
                Id = dbItem.Id,
                Type = dbItem.Type,
                User = user,
                AttachmentNames = dbItem.Files.Select(o => o.Filename).ToList()
            };
        }
    }
}
