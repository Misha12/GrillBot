using Discord;
using Discord.WebSocket;
using Grillbot.Models;
using System.Collections.Generic;

namespace Grillbot.Services.Audit
{
    public class DownloadAuditLogBackgroundTask : BackgroundTask<AuditService>
    {
        public ulong GuildId { get; set; }
        public List<ActionType> ActionTypes { get; }

        public DownloadAuditLogBackgroundTask(SocketGuild guild)
        {
            GuildId = guild.Id;

            ActionTypes = new List<ActionType>()
            {
                ActionType.GuildUpdated,
                ActionType.ChannelCreated,
                ActionType.ChannelDeleted,
                ActionType.ChannelUpdated,
                ActionType.EmojiCreated,
                ActionType.EmojiDeleted,
                ActionType.EmojiUpdated,
                ActionType.OverwriteCreated,
                ActionType.OverwriteDeleted,
                ActionType.OverwriteUpdated,
                ActionType.Prune,
                ActionType.Unban,
                ActionType.MemberUpdated,
                ActionType.MemberRoleUpdated,
                ActionType.BotAdded,
                ActionType.RoleCreated,
                ActionType.RoleDeleted,
                ActionType.RoleUpdated,
                ActionType.WebhookCreated,
                ActionType.WebhookDeleted,
                ActionType.WebhookUpdated,
                ActionType.MessagePinned,
                ActionType.MessageUnpinned
            };
        }
    }
}
