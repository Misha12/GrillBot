using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using Discord;
using Discord.WebSocket;
using Grillbot.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;

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

        public static MessageEditedAuditData Create(ImmutableArray<EmbedField> fields)
        {
            var channelIdRegex = new Regex(@".*\s\((\d*)\)", RegexOptions.IgnoreCase);
            var channelIdMatch = channelIdRegex.Match(fields[3].Value);

            if (!channelIdMatch.Success)
                return null;

            var before = fields[1].Value.ClearCodeBlocks();
            var after = fields[2].Value.ClearCodeBlocks();
            var jumpUrl = fields[4].Value;

            return new MessageEditedAuditData(before, after, Convert.ToUInt64(channelIdMatch.Groups[1].Value), jumpUrl);
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
