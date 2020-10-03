using Discord.WebSocket;
using Grillbot.Database.Entity.Users;
using Grillbot.Services.InviteTracker;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
                    CreatorId = creator?.ID
                };

                Context.Invites.Add(inviteEntity);
            }
        }

        public async Task<List<Invite>> GetInvitesAsync(SocketGuild guild, bool includeUsers, bool asc)
        {
            var codesFromGuild = await Context.Users.AsQueryable()
                .Where(o => o.UsedInviteCode != null && o.GuildID == guild.Id.ToString())
                .Select(o => o.UsedInviteCode)
                .Distinct().ToListAsync();

            var query = Context.Invites.AsQueryable()
                .Include(o => o.Creator)
                .Where(o => codesFromGuild.Contains(o.Code));

            if (asc)
                query = query.OrderBy(o => o.UsedUsers.Count);
            else
                query = query.OrderByDescending(o => o.UsedUsers.Count);

            query = !includeUsers ? query : query.Include(o => o.UsedUsers);
            return await query.ToListAsync();
        }

        public Task<Invite> FindInviteAsync(string code)
        {
            return Context.Invites
                .Include(o => o.Creator)
                .SingleOrDefaultAsync(o => o.Code == code);
        }
    }
}
