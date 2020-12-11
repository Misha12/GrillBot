using Discord;
using Grillbot.Enums;
using Grillbot.Models.Audit;
using Grillbot.Models.Audit.DiscordAuditLog;
using System;
using System.Collections.Generic;

namespace Grillbot.Services.Audit
{
    public static class AuditServiceHelper
    {
        public static Dictionary<ActionType, AuditLogType> AuditLogTypeMap { get; } = new Dictionary<ActionType, AuditLogType>()
        {
            { ActionType.ChannelCreated, AuditLogType.ChannelCreated },
            { ActionType.ChannelDeleted, AuditLogType.ChannelDeleted },
            { ActionType.ChannelUpdated,AuditLogType.ChannelUpdated },
            { ActionType.EmojiCreated ,AuditLogType.EmojiCreated },
            { ActionType.EmojiDeleted, AuditLogType.EmojiDeleted },
            { ActionType.EmojiUpdated, AuditLogType.EmojiUpdated },
            { ActionType.OverwriteCreated, AuditLogType.OverwriteCreated },
            { ActionType.OverwriteDeleted, AuditLogType.OverwriteDeleted },
            { ActionType.OverwriteUpdated, AuditLogType.OverwriteUpdated },
            { ActionType.Prune, AuditLogType.Prune },
            { ActionType.Unban, AuditLogType.Unban },
            { ActionType.MemberUpdated, AuditLogType.MemberUpdated },
            { ActionType.MemberRoleUpdated, AuditLogType.MemberRoleUpdated },
            { ActionType.BotAdded, AuditLogType.BotAdded },
            { ActionType.RoleCreated, AuditLogType.RoleCreated },
            { ActionType.RoleDeleted, AuditLogType.RoleDeleted },
            { ActionType.RoleUpdated, AuditLogType.RoleUpdated },
            { ActionType.WebhookCreated, AuditLogType.WebhookCreated },
            { ActionType.WebhookDeleted, AuditLogType.WebhookDeleted },
            { ActionType.WebhookUpdated, AuditLogType.WebhookUpdated },
            { ActionType.MessagePinned, AuditLogType.MessagePinned },
            { ActionType.MessageUnpinned, AuditLogType.MessageUnpinned },
            { ActionType.GuildUpdated, AuditLogType.GuildUpdated }
        };

        public static Dictionary<ActionType, Func<IAuditLogData, IAuditLogData>> AuditLogDataMap { get; } = new Dictionary<ActionType, Func<IAuditLogData, IAuditLogData>>()
        {
            { ActionType.ChannelCreated, AuditChannelInfo.Create },
            { ActionType.ChannelDeleted, AuditChannelInfo.Create },
            { ActionType.ChannelUpdated, AuditChannelUpdated.Create },
            { ActionType.EmojiCreated, AuditEmoteInfo.Create },
            { ActionType.EmojiDeleted, AuditEmoteInfo.Create },
            { ActionType.EmojiUpdated, AuditEmoteUpdated.Create },
            { ActionType.Prune, AuditPruneMembers.Create },
            { ActionType.Unban, AuditUnban.Create },
            { ActionType.MessagePinned, AuditMessagePinInfo.Create },
            { ActionType.MessageUnpinned, AuditMessagePinInfo.Create },
            { ActionType.BotAdded, AuditBotAdded.Create },
            { ActionType.OverwriteCreated, AuditOverwriteInfo.Create },
            { ActionType.OverwriteDeleted, AuditOverwriteInfo.Create },
            { ActionType.OverwriteUpdated, AuditOverwriteUpdated.Create },
            { ActionType.MemberUpdated, AuditMemberUpdated.Create },
            { ActionType.MemberRoleUpdated, AuditMemberUpdated.Create },
            { ActionType.RoleCreated, Role.Create },
            { ActionType.RoleDeleted, Role.Create },
            { ActionType.RoleUpdated, RoleUpdated.Create },
            { ActionType.WebhookCreated, Webhook.Create },
            { ActionType.WebhookDeleted, Webhook.Create },
            { ActionType.WebhookUpdated, WebhookUpdated.Create },
            { ActionType.GuildUpdated, GuildUpdated.Create }
        };

        public static bool IsTypeDefined(ActionType type)
        {
            return AuditLogTypeMap.ContainsKey(type) && AuditLogDataMap.ContainsKey(type);
        }
    }
}
