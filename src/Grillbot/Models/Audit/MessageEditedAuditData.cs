using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using Discord;
using Newtonsoft.Json;
using System.Linq;

namespace Grillbot.Models.Audit
{
    public class MessageEditedAuditData
    {
        [JsonProperty("before")]
        public string Before { get; set; }

        [JsonProperty("after")]
        public string After { get; set; }

        [JsonProperty("url")]
        public string JumpUrl { get; set; }

        public MessageEditedAuditData() { }

        public MessageEditedAuditData(string before, string after, string jumpUrl)
        {
            Before = before;
            After = after;
            JumpUrl = jumpUrl;
        }

        public static MessageEditedAuditData Create(IMessage before, IMessage after)
        {
            return new MessageEditedAuditData(before.Content, after.Content, after.GetJumpUrl());
        }

        public string CreateDiff()
        {
            var diffResult = InlineDiffBuilder.Diff(Before ?? "", After ?? "", false);
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
