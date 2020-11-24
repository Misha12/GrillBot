using Grillbot.Database.Entity.Users;
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

        public IQueryable<Invite> GetInvitesQuery(ulong guildID, DateTime? createdFrom, DateTime? createdTo, List<long> creatorUserIds, bool desc)
        {
            var codesFromGuild = Context.Users.AsQueryable()
                .Where(o => o.UsedInviteCode != null && o.GuildID == guildID.ToString())
                .Select(o => o.UsedInviteCode)
                .Distinct();

            var query = Context.Invites.AsQueryable()
                .Include(o => o.Creator)
                .Include(o => o.UsedUsers)
                .Where(o => codesFromGuild.Contains(o.Code));

            if (createdFrom != null)
                query = query.Where(o => o.CreatedAt >= createdFrom.Value);

            if (createdTo != null)
                query = query.Where(o => o.CreatedAt < createdTo);

            if (creatorUserIds != null && creatorUserIds.Count > 0)
                query = query.Where(o => o.CreatorId != null && creatorUserIds.Contains(o.CreatorId.Value));

            if (!desc)
                return query.OrderBy(o => o.UsedUsers.Count);
            else
                return query.OrderByDescending(o => o.UsedUsers.Count);
        }

        public Task<Invite> FindInviteAsync(string code)
        {
            return Context.Invites
                .Include(o => o.Creator)
                .SingleOrDefaultAsync(o => o.Code == code);
        }

        public IQueryable<Invite> GetInvitesOfUser(long id)
        {
            return Context.Invites
                .Include(o => o.UsedUsers)
                .Where(o => o.CreatorId == id);
        }
    }
}
