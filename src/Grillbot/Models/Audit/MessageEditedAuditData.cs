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
        public ulong ChannelId { get; set; }
        public string Before { get; set; }
        public string After { get; set; }

        [JsonIgnore]
        public IChannel Channel { get; set; }

        public string JumpUrl { get; set; }

        public static MessageEditedAuditData CreateDbItem(IChannel channel, IMessage before, IMessage after)
        {
            return new MessageEditedAuditData()
            {
                After = after.Content,
                Before = before.Content,
                ChannelId = channel.Id,
                JumpUrl = after.GetJumpUrl()
            };
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
