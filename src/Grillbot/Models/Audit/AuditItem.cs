using Discord;
using Discord.WebSocket;
using Grillbot.Database.Entity.AuditLog;
using Grillbot.Enums;
using Grillbot.Models.Audit.DiscordAuditLog;
using Grillbot.Models.Users;
using Grillbot.Services.MessageCache;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Models.Audit
{
    public class AuditItem
    {
        public long Id { get; set; }
        public DiscordUser User { get; set; }
        public DateTime CreatedAt { get; set; }
        public AuditLogType Type { get; set; }
        public List<string> AttachmentNames { get; set; }
        public IGuildChannel Channel { get; set; }

        #region Typed properties

        public CommandAuditData CommandAuditData { get; set; }
        public UserLeftAuditData UserLeftAuditData { get; set; }
        public UserJoinedAuditData UserJoinedAuditData { get; set; }
        public MessageEditedAuditData MessageEditedAuditData { get; set; }
        public MessageDeletedAuditData MessageDeletedAuditData { get; set; }
        public AuditBotAdded BotAdded { get; set; }
        public AuditChannelInfo ChannelInfo { get; set; }
        public GuildUpdated GuildUpdated { get; set; }
        public AuditChannelUpdated ChannelUpdated { get; set; }
        public AuditEmoteInfo EmoteInfo { get; set; }
        public AuditEmoteUpdated EmoteUpdated { get; set; }
        public AuditOverwriteInfo OverwriteInfo { get; set; }
        public AuditOverwriteUpdated OverwriteUpdated { get; set; }
        public AuditPruneMembers PruneMembers { get; set; }
        public AuditUnban Unban { get; set; }
        public AuditMemberUpdated MemberUpdated { get; set; }
        public Role Role { get; set; }
        public RoleUpdated RoleUpdated { get; set; }
        public Webhook Webhook { get; set; }
        public WebhookUpdated WebhookUpdated { get; set; }
        public AuditMessagePinInfo PinInfo { get; set; }

        #endregion

        public static async Task<AuditItem> CreateAsync(SocketGuild guild, AuditLogItem dbItem, DiscordUser user, IMessageCache cache)
        {
            var item = new AuditItem()
            {
                CreatedAt = dbItem.CreatedAt,
                Id = dbItem.Id,
                Type = dbItem.Type,
                User = user,
                AttachmentNames = dbItem.Files.Select(o => o.Filename).ToList(),
                Channel = dbItem.ChannelIdSnowflake != null ? guild.GetChannel(dbItem.ChannelIdSnowflake.Value) : null
            };

            switch (dbItem.Type)
            {
                case AuditLogType.BotAdded:
                    item.BotAdded = JsonConvert.DeserializeObject<AuditBotAdded>(dbItem.JsonData);
                    break;
                case AuditLogType.ChannelCreated:
                case AuditLogType.ChannelDeleted:
                    item.ChannelInfo = JsonConvert.DeserializeObject<AuditChannelInfo>(dbItem.JsonData);
                    break;
                case AuditLogType.ChannelUpdated:
                    item.ChannelUpdated = JsonConvert.DeserializeObject<AuditChannelUpdated>(dbItem.JsonData);
                    break;
                case AuditLogType.Command:
                    item.CommandAuditData = JsonConvert.DeserializeObject<CommandAuditData>(dbItem.JsonData);
                    break;
                case AuditLogType.EmojiCreated:
                case AuditLogType.EmojiDeleted:
                    item.EmoteInfo = JsonConvert.DeserializeObject<AuditEmoteInfo>(dbItem.JsonData);
                    break;
                case AuditLogType.EmojiUpdated:
                    item.EmoteUpdated = JsonConvert.DeserializeObject<AuditEmoteUpdated>(dbItem.JsonData);
                    break;
                case AuditLogType.GuildUpdated:
                    item.GuildUpdated = JsonConvert.DeserializeObject<GuildUpdated>(dbItem.JsonData).GetFilledModel(guild);
                    break;
                case AuditLogType.MemberRoleUpdated:
                case AuditLogType.MemberUpdated:
                    item.MemberUpdated = await JsonConvert.DeserializeObject<AuditMemberUpdated>(dbItem.JsonData).GetFilledModelAsync(guild);
                    break;
                case AuditLogType.MessageDeleted:
                    item.MessageDeletedAuditData = JsonConvert.DeserializeObject<MessageDeletedAuditData>(dbItem.JsonData).GetFilledModel(guild);
                    break;
                case AuditLogType.MessageEdited:
                    item.MessageEditedAuditData = JsonConvert.DeserializeObject<MessageEditedAuditData>(dbItem.JsonData);
                    break;
                case AuditLogType.MessagePinned:
                case AuditLogType.MessageUnpinned:
                    item.PinInfo = await JsonConvert.DeserializeObject<AuditMessagePinInfo>(dbItem.JsonData).GetFilledModelAsync(guild, cache);
                    break;
                case AuditLogType.OverwriteCreated:
                case AuditLogType.OverwriteDeleted:
                    item.OverwriteInfo = JsonConvert.DeserializeObject<AuditOverwriteInfo>(dbItem.JsonData).GetFilledModel(guild);
                    break;
                case AuditLogType.OverwriteUpdated:
                    item.OverwriteUpdated = JsonConvert.DeserializeObject<AuditOverwriteUpdated>(dbItem.JsonData).GetFilledModel(guild);
                    break;
                case AuditLogType.Prune:
                    item.PruneMembers = JsonConvert.DeserializeObject<AuditPruneMembers>(dbItem.JsonData);
                    break;
                case AuditLogType.RoleCreated:
                case AuditLogType.RoleDeleted:
                    item.Role = JsonConvert.DeserializeObject<Role>(dbItem.JsonData);
                    break;
                case AuditLogType.RoleUpdated:
                    item.RoleUpdated = JsonConvert.DeserializeObject<RoleUpdated>(dbItem.JsonData);
                    break;
                case AuditLogType.Unban:
                    item.Unban = JsonConvert.DeserializeObject<AuditUnban>(dbItem.JsonData);
                    break;
                case AuditLogType.UserJoined:
                    item.UserJoinedAuditData = JsonConvert.DeserializeObject<UserJoinedAuditData>(dbItem.JsonData).GetFilledModel(user);
                    break;
                case AuditLogType.UserLeft:
                    item.UserLeftAuditData = JsonConvert.DeserializeObject<UserLeftAuditData>(dbItem.JsonData);
                    break;
                case AuditLogType.WebhookCreated:
                case AuditLogType.WebhookDeleted:
                    item.Webhook = JsonConvert.DeserializeObject<Webhook>(dbItem.JsonData);
                    break;
                case AuditLogType.WebhookUpdated:
                    item.WebhookUpdated = JsonConvert.DeserializeObject<WebhookUpdated>(dbItem.JsonData).GetFilledModel(guild);
                    break;
            }

            return item;
        }
    }
}
