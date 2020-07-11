using Grillbot.Database.Entity.Users;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System;
using Grillbot.Enums;

namespace Grillbot.Database.Repository
{
    public class UsersRepository : RepositoryBase
    {
        public UsersRepository(GrillBotContext context) : base(context)
        {
        }

        private IQueryable<DiscordUser> GetBaseQuery(bool includeChannels, bool includeBirthday, bool includeMathAudit,
            bool includeStatistics, bool includeReminders)
        {
            var query = Context.Users.AsQueryable();

            if (includeChannels)
                query = query.Include(o => o.Channels);

            if (includeBirthday)
                query = query.Include(o => o.Birthday);

            if (includeMathAudit)
                query = query.Include(o => o.MathAudit);

            if (includeStatistics)
                query = query.Include(o => o.Statistics);

            if (includeReminders)
                query = query.Include(o => o.Reminders);

            return query;
        }

        public IQueryable<DiscordUser> GetUsers(WebAdminUserOrder order, bool desc, ulong? guildID, int limit, List<ulong> userIds)
        {
            var query = GetBaseQuery(true, false, false, false, false);

            if (guildID != null)
                query = query.Where(o => o.GuildID == guildID.ToString());

            if (userIds != null)
            {
                var ids = userIds.Select(o => o.ToString()).ToList();
                query = query.Where(o => ids.Contains(o.UserID));
            }

            return OrderUsers(query, desc, order).Take(limit);
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

        public DiscordUser GetUser(ulong guildID, ulong userID, bool includeChannels, bool includeBirthday, bool includeMathAudit, bool includeStatistics,
            bool includeReminders)
        {
            var guild = guildID.ToString();
            var user = userID.ToString();

            var query = GetBaseQuery(includeChannels, includeBirthday, includeMathAudit, includeStatistics, includeReminders);
            return query.FirstOrDefault(o => o.GuildID == guild && o.UserID == user);
        }

        public DiscordUser GetUserDetail(long id)
        {
            var query = GetBaseQuery(true, true, true, true, true);
            return query.FirstOrDefault(o => o.ID == id);
        }

        public async Task<long?> FindUserIDFromDiscordIDAsync(ulong guildID, ulong userID)
        {
            var guild = guildID.ToString();
            var user = userID.ToString();

            var query = GetBaseQuery(false, false, false, false, false);
            var entity = await query
                .SingleOrDefaultAsync(o => o.GuildID == guild && o.UserID == user);

            return entity?.ID;
        }

        public DiscordUser GetOrCreateUser(ulong guildID, ulong userID, bool includeChannels, bool includeBirthday, bool includeMathAudit,
            bool includeStatistics, bool includeReminders)
        {
            var entity = GetUser(guildID, userID, includeChannels, includeBirthday, includeMathAudit, includeStatistics, includeReminders);

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

        public async Task<List<string>> GetUsersForFilterAsync()
        {
            return await GetBaseQuery(false, false, false, false, false)
                .Select(o => o.UserID)
                .Distinct()
                .ToListAsync();
        }

        public DiscordUser FindUserByApiToken(string apiToken)
        {
            var query = GetBaseQuery(false, false, false, false, false);
            return query.FirstOrDefault(o => o.ApiToken == apiToken);
        }

        public List<DiscordUser> GetUsersWithBirthday(ulong guildID)
        {
            var guild = guildID.ToString();

            return GetBaseQuery(false, true, false, false, false)
                .Where(o => o.GuildID == guild && o.Birthday != null)
                .ToList();
        }

        public int CalculatePointsPosition(ulong guildID, long points)
        {
            var pointsList = GetBaseQuery(false, false, false, false, false)
                .Where(o => o.GuildID == guildID.ToString())
                .OrderByDescending(o => o.Points)
                .ThenBy(o => o.ID)
                .Select(o => o.Points)
                .ToList();

            return pointsList.FindIndex(o => o == points);
        }

        public IQueryable<DiscordUser> GetUsersWithPointsOrder(ulong guildID, int skip, int take, bool asc)
        {
            var query = GetBaseQuery(false, false, false, false, false)
                .Where(o => o.GuildID == guildID.ToString());

            if (asc)
                query = query.OrderBy(o => o.Points).ThenByDescending(o => o.ID);
            else
                query = query.OrderByDescending(o => o.Points).ThenBy(o => o.ID);

            return query
                .Skip(skip)
                .Take(take);
        }
    }
}
