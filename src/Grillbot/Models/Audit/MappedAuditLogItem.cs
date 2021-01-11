using Discord;

namespace Grillbot.Models.Audit
{
    public class MappedAuditLogItem
    {
        public ulong? ChannelId { get; set; }
        public IAuditLogData Data { get; set; }

        public MappedAuditLogItem(ulong? channelId, IAuditLogData data)
        {
            ChannelId = channelId;
            Data = data;
        }
    }
}
