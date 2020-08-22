using Discord;
using Discord.Rest;
using Grillbot.Database.Entity.Users;
using System;

namespace Grillbot.Services.InviteTracker
{
    public class InviteModel
    {
        public string Code { get; set; }
        public ulong ChannelId { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
        public IUser Creator { get; set; }
        public int? Uses { get; set; }

        public InviteModel(Invite entity, IUser creator, int? uses = null)
        {
            Code = entity.Code;
            ChannelId = entity.ChannelIdSnowflake;
            CreatedAt = entity.CreatedAt.HasValue ? new DateTimeOffset(entity.CreatedAt.Value) : (DateTimeOffset?)null;
            Creator = creator;
            Uses = uses;
        }

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
