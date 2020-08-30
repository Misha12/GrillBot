using Grillbot.Database.Entity.Users;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System;
using Grillbot.Enums;
using Grillbot.Database.Enums.Includes;

namespace Grillbot.Database.Repository
{
    public class UsersRepository : RepositoryBase
    {
        public UsersRepository(GrillBotContext context) : base(context)
        {
        }

        private IQueryable<DiscordUser> GetBaseQuery(UsersIncludes includes)
        {
            var query = Context.Users.AsQueryable();

            if (includes.HasFlag(UsersIncludes.Channels))
                query = query.Include(o => o.Channels);

            if (includes.HasFlag(UsersIncludes.Birthday))
                query = query.Include(o => o.Birthday);

            if (includes.HasFlag(UsersIncludes.MathAudit))
                query = query.Include(o => o.MathAudit);

            if (includes.HasFlag(UsersIncludes.Statistics))
                query = query.Include(o => o.Statistics);

            if (includes.HasFlag(UsersIncludes.Reminders))
                query = query.Include(o => o.Reminders);

            if (includes.HasFlag(UsersIncludes.Invites))
            {
                query = query
                    .Include(o => o.CreatedInvites)
                    .Include(o => o.UsedInvite)
                    .ThenInclude(o => o.Creator);
            }

            if (includes.HasFlag(UsersIncludes.Emotes))
                query = query.Include(o => o.UsedEmotes);

            if (includes.HasFlag(UsersIncludes.Unverify))
            {
                query = query
                    .Include(o => o.Unverify)
                    .ThenInclude(o => o.SetLogOperation)
                    .ThenInclude(o => o.FromUser);
            }

            if (includes.HasFlag(UsersIncludes.UnverifyLogIncoming))
            {
                query = query
                    .Include(o => o.IncomingUnverifyOperations)
                    .ThenInclude(o => o.FromUser);
            }

            if (includes.HasFlag(UsersIncludes.UnverifyLogOutgoing))
            {
                query = query
                    .Include(o => o.OutgoingUnverifyOperations)
                    .ThenInclude(o => o.ToUser);
            }

            return query;
        }

        public IQueryable<DiscordUser> GetUsers(WebAdminUserOrder order, bool desc, ulong guildID, List<ulong> userIds, string usedInviteCode, int skip, int take)
        {
            var query = GetBaseQuery(UsersIncludes.Channels | UsersIncludes.Invites)
                .Where(o => o.GuildID == guildID.ToString());

            if (userIds.Count > 0)
            {
                var ids = userIds.Select(o => o.ToString()).ToList();
                query = query.Where(o => ids.Contains(o.UserID));
            }

            if (!string.IsNullOrEmpty(usedInviteCode))
                query = query.Where(o => o.UsedInviteCode.Contains(usedInviteCode));

            return OrderUsers(query, desc, order).Skip(skip).Take(take);
        }

        private IQueryable<DiscordUser> OrderUsers(IQueryable<DiscordUser> query, bool desc, WebAdminUserOrder order)
        {
            if (order == WebAdminUserOrder.UserID)
                return desc ? query.OrderByDescending(o => o.ID) : query.OrderBy(o => o.ID);

            Expression<Func<DiscordUser, object>> expression = order switch
            {
                WebAdminUserOrder.MessageCount => o => o.Channels.Sum(x => x.Count),
                WebAdminUserOrder.GivenReactions => o => o.GivenReactionsCount,
                WebAdminUserOrder.ObtainedReactions => o => o.ObtainedReactionsCount,
                WebAdminUserOrder.Server => o => o.GuildID,
                _ => o => o.Points,
            };

            if (desc)
            {
                return query
                    .OrderByDescending(expression)
                    .ThenByDescending(o => o.ID);
            }

            return query
                .OrderBy(expression)
                .ThenBy(o => o.ID);
        }

        public DiscordUser GetUser(ulong guildID, ulong userID, UsersIncludes includes)
        {
            var guild = guildID.ToString();
            var user = userID.ToString();

            return GetBaseQuery(includes)
                .FirstOrDefault(o => o.GuildID == guild && o.UserID == user);
        }

        public Task<DiscordUser> GetUserAsync(long userID, UsersIncludes includes)
        {
            return GetBaseQuery(includes)
                .FirstOrDefaultAsync(o => o.ID == userID);
        }

        public async Task<long?> FindUserIDFromDiscordIDAsync(ulong guildID, ulong userID)
        {
            var guild = guildID.ToString();
            var user = userID.ToString();

            var entity = await GetBaseQuery(UsersIncludes.None)
                .Select(o => new { o.GuildID, o.UserID, o.ID })
                .SingleOrDefaultAsync(o => o.GuildID == guild && o.UserID == user);

            return entity?.ID;
        }

        public DiscordUser GetOrCreateUser(ulong guildID, ulong userID, UsersIncludes includes)
        {
            var entity = GetUser(guildID, userID, includes);

            if (entity == null)
            {
                entity = new DiscordUser()
                {
                    GuildIDSnowflake = guildID,
                    UserIDSnowflake = userID
                };

                Context.Users.Add(entity);
            }

            return entity;
        }

        public DiscordUser FindUserByApiToken(string apiToken)
        {
            return GetBaseQuery(UsersIncludes.None)
                .FirstOrDefault(o => o.ApiToken == apiToken);
        }

        public List<DiscordUser> GetUsersWithBirthday(ulong guildID)
        {
            var guild = guildID.ToString();

            return GetBaseQuery(UsersIncludes.Birthday)
                .Where(o => o.GuildID == guild && o.Birthday != null)
                .ToList();
        }

        public int CalculatePointsPosition(ulong guildID, long userID)
        {
            var pointsList = GetBaseQuery(UsersIncludes.None)
                .Where(o => o.GuildID == guildID.ToString() && o.Points > 0)
                .OrderByDescending(o => o.Points)
                .ThenBy(o => o.ID)
                .Select(o => new { o.ID, o.Points })
                .ToList();

            return pointsList.FindIndex(o => o.ID == userID);
        }

        public IQueryable<DiscordUser> GetUsersWithPointsOrder(ulong guildID, int skip, int take, bool asc)
        {
            var query = GetBaseQuery(UsersIncludes.None)
                .Where(o => o.GuildID == guildID.ToString() && o.Points > 0);

            if (asc)
                query = query.OrderBy(o => o.Points).ThenByDescending(o => o.ID);
            else
                query = query.OrderByDescending(o => o.Points).ThenBy(o => o.ID);

            return query
                .Skip(skip)
                .Take(take);
        }

        public IQueryable<DiscordUser> GetUsersWithUsedCode(ulong guildID, string code)
        {
            return Context.Users.AsQueryable()
                .Where(o => o.GuildID == guildID.ToString() && o.UsedInviteCode == code);
        }

        public IQueryable<DiscordUser> GetUsersWithUnverify(ulong guildID)
        {
            return GetBaseQuery(UsersIncludes.Unverify)
                .Where(o => o.GuildID == guildID.ToString() && o.Unverify != null);
        }
    }
}
