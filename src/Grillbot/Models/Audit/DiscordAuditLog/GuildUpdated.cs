using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Grillbot.Extensions.Discord;
using Newtonsoft.Json;

namespace Grillbot.Models.Audit.DiscordAuditLog
{
    public class GuildUpdated : IAuditLogData
    {
        [JsonProperty("afk_t")]
        public DiffData<int?> AfkTimeout { get; set; }

        [JsonProperty("def_msg_notifications")]
        public DiffData<DefaultMessageNotifications?> DefaultMessageNotifications { get; set; }

        [JsonProperty("afk_ch_id")]
        public DiffData<ulong?> AfkChannelId { get; set; }

        [JsonIgnore]
        public DiffData<IChannel> AfkChannel { get; set; }

        [JsonProperty("name")]
        public DiffData<string> Name { get; set; }

        [JsonProperty("region")]
        public DiffData<string> RegionId { get; set; }

        [JsonProperty("icon")]
        public DiffData<string> IconHash { get; set; }

        [JsonProperty("verify")]
        public DiffData<VerificationLevel?> VerificationLevel { get; set; }

        [JsonProperty("owner")]
        public DiffData<ulong> OwnerId { get; set; }

        [JsonIgnore]
        public DiffData<IUser> Owner { get; set; }

        [JsonProperty("mfa")]
        public DiffData<MfaLevel?> MfaLevel { get; set; }

        [JsonProperty("content_filter")]
        public DiffData<ExplicitContentFilterLevel?> ExplicitContentFilter { get; set; }

        [JsonProperty("sys_ch_id")]
        public DiffData<ulong?> SystemChannelId { get; set; }

        [JsonIgnore]
        public DiffData<IChannel> SystemChannel { get; set; }

        [JsonProperty("emb_ch_id")]
        public DiffData<ulong?> EmbedChannelId { get; set; }

        [JsonIgnore]
        public DiffData<IChannel> EmbedChannel { get; set; }

        [JsonProperty("embedable")]
        public DiffData<bool> IsEmbedable { get; set; }

        public static IAuditLogData Create(IAuditLogData entryData)
        {
            if (entryData is not GuildUpdateAuditLogData data)
                return null;

            var item = new GuildUpdated()
            {
                AfkChannelId = data.Before.AfkChannelId != data.After.AfkChannelId ? new DiffData<ulong?>(data.Before.AfkChannelId, data.After.AfkChannelId) : null,
                AfkTimeout = data.Before.AfkTimeout != data.After.AfkTimeout ? new DiffData<int?>(data.Before.AfkTimeout, data.After.AfkTimeout) : null,
                EmbedChannelId = data.Before.EmbedChannelId != data.After.EmbedChannelId ? new DiffData<ulong?>(data.Before.EmbedChannelId, data.After.EmbedChannelId) : null,
                IconHash = data.Before.IconHash != data.After.IconHash ? new DiffData<string>(data.Before.IconHash, data.After.IconHash) : null,
                IsEmbedable = data.Before.IsEmbeddable != data.After.IsEmbeddable ? new DiffData<bool>(data.Before.IsEmbeddable ?? false, data.After.IsEmbeddable ?? false) : null,
                MfaLevel = data.Before.MfaLevel != data.After.MfaLevel ? new DiffData<MfaLevel?>(data.Before.MfaLevel, data.After.MfaLevel) : null,
                Name = data.Before.Name != data.After.Name ? new DiffData<string>(data.Before.Name, data.After.Name) : null,
                OwnerId = data.Before.Owner?.Id != data.After.Owner?.Id ? new DiffData<ulong>(data.Before.Owner.Id, data.After.Owner.Id) : null,
                RegionId = data.Before.RegionId != data.After.RegionId ? new DiffData<string>(data.Before.RegionId, data.After.RegionId) : null,
                SystemChannelId = data.Before.SystemChannelId != data.After.SystemChannelId ? new DiffData<ulong?>(data.Before.SystemChannelId, data.After.SystemChannelId) : null,
                VerificationLevel = data.Before.VerificationLevel != data.After.VerificationLevel ? new DiffData<VerificationLevel?>(data.Before.VerificationLevel, data.After.VerificationLevel) : null
            };

            if(data.Before.DefaultMessageNotifications != data.After.DefaultMessageNotifications)
                item.DefaultMessageNotifications = new DiffData<DefaultMessageNotifications?>(data.Before.DefaultMessageNotifications, data.After.DefaultMessageNotifications);

            if (data.Before.ExplicitContentFilter != data.After.ExplicitContentFilter)
                item.ExplicitContentFilter = new DiffData<ExplicitContentFilterLevel?>(data.Before.ExplicitContentFilter, data.After.ExplicitContentFilter);

            return item;
        }

        public GuildUpdated GetFilledModel(SocketGuild guild)
        {
            if (AfkChannelId != null)
            {
                var oldChannel = guild.GetChannel(AfkChannelId.Before ?? 0);
                var newChannel = guild.GetChannel(AfkChannelId.After ?? 0);

                AfkChannel = new DiffData<IChannel>(oldChannel, newChannel);
            }

            if (OwnerId != null)
            {
                var oldOwner = guild.GetUserFromGuildAsync(OwnerId.Before);
                var newOwner = guild.GetUserFromGuildAsync(OwnerId.After);

                Owner = new DiffData<IUser>(oldOwner.Result, newOwner.Result);
            }

            if (SystemChannelId != null)
            {
                var oldSystemChannel = guild.GetChannel(SystemChannelId.Before ?? 0);
                var newSystemChannel = guild.GetChannel(SystemChannelId.After ?? 0);

                SystemChannel = new DiffData<IChannel>(oldSystemChannel, newSystemChannel);
            }

            if(EmbedChannelId != null)
            {
                var oldEmbedChannel = guild.GetChannel(EmbedChannelId.Before ?? 0);
                var newEmbedChannel = guild.GetChannel(EmbedChannelId.After ?? 0);

                EmbedChannel = new DiffData<IChannel>(oldEmbedChannel, newEmbedChannel);
            }

            return this;
        }
    }
}
