using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace Grillbot.Models.Audit.DiscordAuditLog
{
    public class WebhookUpdated : IAuditLogData
    {
        [JsonProperty("ch_id")]
        public DiffData<ulong> ChannelId { get; set; }

        [JsonIgnore]
        public DiffData<IChannel> Channel { get; set; }

        [JsonProperty("name")]
        public DiffData<string> Name { get; set; }

        public static IAuditLogData Create(IAuditLogData entryData)
        {
            if (entryData is not WebhookUpdateAuditLogData data)
                return null;

            return new WebhookUpdated()
            {
                ChannelId = data.Before.ChannelId != data.After.ChannelId ? new DiffData<ulong>(data.Before.ChannelId ?? 0, data.After.ChannelId ?? 0) : null,
                Name = data.Before.Name != data.After.Name ? new DiffData<string>(data.Before.Name, data.After.Name) : null
            };
        }

        public WebhookUpdated GetFilledModel(SocketGuild guild)
        {
            if (ChannelId == null)
                return this;

            var oldChannel = guild.GetChannel(ChannelId.Before);
            var newChannel = guild.GetChannel(ChannelId.After);

            Channel = new DiffData<IChannel>(oldChannel, newChannel);
            return this;
        }
    }
}
