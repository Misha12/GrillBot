using Discord.Rest;
using System;

namespace Grillbot.Services.InviteTracker
{
    public class InviteModel
    {
        public string Code { get; set; }
        public ulong ChannelId { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
        public RestUser Creator { get; set; }
        public int? Uses { get; set; }

        public InviteModel(RestInviteMetadata metadata)
        {
            Code = metadata.Code;
            ChannelId = metadata.ChannelId;
            CreatedAt = metadata.CreatedAt;
            Creator = metadata.Inviter;
            Uses = metadata.Uses;
        }
    }
}
