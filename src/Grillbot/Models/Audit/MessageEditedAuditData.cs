using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System.Linq;

namespace Grillbot.Models.Audit
{
    public class MessageEditedAuditData
    {
        [JsonProperty("ch_id")]
        public ulong ChannelId { get; set; }

        [JsonProperty("before")]
        public string Before { get; set; }

        [JsonProperty("after")]
        public string After { get; set; }

        [JsonIgnore]
        public IChannel Channel { get; set; }

        [JsonProperty("url")]
        public string JumpUrl { get; set; }

        public MessageEditedAuditData() { }

        public MessageEditedAuditData(string before, string after, ulong channelId, string jumpUrl)
        {
            Before = before;
            After = after;
            ChannelId = channelId;
            JumpUrl = jumpUrl;
        }

        public static MessageEditedAuditData Create(IChannel channel, IMessage before, IMessage after)
        {
            return new MessageEditedAuditData(before.Content, after.Content, channel.Id, after.GetJumpUrl());
        }

        public MessageEditedAuditData GetFilledModel(SocketGuild guild)
        {
            Channel = guild.GetChannel(ChannelId);

            return this;
        }

        public string CreateDiff()
        {
            var diffResult = InlineDiffBuilder.Diff(Before, After, false);
            var diff = diffResult.Lines.Select(o =>
            {
                return o.Type switch
                {
                    ChangeType.Inserted => $"+ {o.Text}",
                    ChangeType.Deleted => $"- {o.Text}",
                    _ => null,
                };
            }).Where(o => o != null);

            return string.Join("\n", diff);
        }
    }
}
