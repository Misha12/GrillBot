using Grillbot.Database.Entity.Users;
using Grillbot.Services.InviteTracker;
using System;
using System.Linq;

namespace Grillbot.Database.Repository
{
    public class InviteRepository : RepositoryBase
    {
        public InviteRepository(GrillBotContext context) : base(context)
        {
        }

        public void StoreInviteIfNotExists(InviteModel invite, DiscordUser creator)
        {
            var inviteEntity = Context.Invites.FirstOrDefault(o => o.Code == invite.Code);

            if (inviteEntity == null)
            {
                inviteEntity = new Invite()
                {
                    ChannelIdSnowflake = invite.ChannelId,
                    Code = invite.Code,
                    CreatedAt = invite.CreatedAt.HasValue ? invite.CreatedAt.Value.UtcDateTime : (DateTime?)null,
                    CreatorId = creator.ID
                };

                Context.Invites.Add(inviteEntity);
            }
        }
    }
}
