using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Grillbot.Enums;
using Grillbot.Database.Enums.Includes;
using UserEntity = Grillbot.Database.Entity.Users.DiscordUser;
using Grillbot.Database.Enums;
using Microsoft.Data.SqlClient;
using Grillbot.Models.Users;
using Grillbot.Database.Entity.Users.Reporting;
using System.Collections.Generic;

namespace Grillbot.Database.Repository
{
    public class UsersRepository : RepositoryBase
    {
        public UsersRepository(GrillBotContext context) : base(context)
        {
        }

        private IQueryable<UserEntity> GetBaseQuery(UsersIncludes includes, bool noTracking)
        {
            var query = Context.Users.AsQueryable();

            if (includes == UsersIncludes.None)
                return query;

            if ((includes & UsersIncludes.Channels) != 0)
                query = query.Include(o => o.Channels);

            if ((includes & UsersIncludes.Reminders) != 0)
                query = query.Include(o => o.Reminders);

            if ((includes & UsersIncludes.CreatedInvites) != 0)
            {
                query = query
                    .Include(o => o.CreatedInvites);
            }

            if ((includes & UsersIncludes.UsedInvite) != 0)
            {
                query = query
                    .Include(o => o.UsedInvite)
                    .ThenInclude(o => o.Creator);
            }

            if ((includes & UsersIncludes.Emotes) != 0)
                query = query.Include(o => o.UsedEmotes);

            if ((includes & UsersIncludes.Unverify) != 0)
            {
                query = query
                    .Include(o => o.Unverify)
                    .ThenInclude(o => o.SetLogOperation)
                    .ThenInclude(o => o.FromUser);
            }

            if ((includes & UsersIncludes.UnverifyLogIncoming) != 0)
            {
                query = query
                    .Include(o => o.IncomingUnverifyOperations)
                    .ThenInclude(o => o.FromUser);
            }

            if ((includes & UsersIncludes.UnverifyLogOutgoing) != 0)
            {
                query = query
                    .Include(o => o.OutgoingUnverifyOperations)
                    .ThenInclude(o => o.ToUser);
            }

            return noTracking ? query.AsNoTracking() : query;
        }

        public IQueryable<UserEntity> GetUsersQuery(UserListFilter filter, List<long> userIds, UsersIncludes includes)
        {
            var query = GetBaseQuery(includes, false)
                .Where(o => o.GuildID == filter.Guild.Id.ToString());

            if (userIds != null)
                query = query.Where(o => userIds.Contains(o.ID));

            if (!string.IsNullOrEmpty(filter.InviteCode))
                query = query.Where(o => o.UsedInviteCode.Contains(filter.InviteCode));

            if (filter.OnlyWebAdmin)
                query = query.Where(o => o.WebAdminPassword != null);

            if (filter.OnlyApiAccess)
                query = query.Where(o => o.ApiToken != null);

            if (filter.OnlyBotAdmin)
                query = query.Where(o => (o.Flags & (int)UserFlags.BotAdmin) != 0);

            return OrderUsers(query, filter.Desc, filter.Order);
        }

        private IQueryable<UserEntity> OrderUsers(IQueryable<UserEntity> query, bool desc, WebAdminUserOrder order)
        {
            return order switch
            {
                WebAdminUserOrder.GivenReactions when desc => query.OrderByDescending(o => o.GivenReactionsCount).ThenByDescending(o => o.ID),
                WebAdminUserOrder.GivenReactions when !desc => query.OrderBy(o => o.GivenReactionsCount).ThenBy(o => o.ID),
                WebAdminUserOrder.ObtainedReactions when desc => query.OrderByDescending(o => o.ObtainedReactionsCount).ThenByDescending(o => o.ID),
                WebAdminUserOrder.ObtainedReactions when !desc => query.OrderBy(o => o.ObtainedReactionsCount).ThenBy(o => o.ID),
                WebAdminUserOrder.Points when desc => query.OrderByDescending(o => o.Points).ThenByDescending(o => o.ID),
                WebAdminUserOrder.Points when !desc => query.OrderBy(o => o.Points).ThenBy(o => o.ID),
                WebAdminUserOrder.Server when desc => query.OrderByDescending(o => o.GuildID).ThenByDescending(o => o.ID),
                _ => query,
            };
        }

        public Task<UserEntity> GetUserAsync(ulong guildID, ulong userID, UsersIncludes includes, bool onlyRead = false)
        {
            return GetBaseQuery(includes, onlyRead)
                .SingleOrDefaultAsync(o => o.GuildID == guildID.ToString() && o.UserID == userID.ToString());
        }

        public Task<UserEntity> GetUserAsync(long userID, UsersIncludes includes, bool asNoTracking = false)
        {
            var query = GetBaseQuery(includes, asNoTracking)
                .AsSplitQuery();

            return query
                .FirstOrDefaultAsync(o => o.ID == userID);
        }

        public async Task<long?> FindUserIDFromDiscordIDAsync(ulong guildID, ulong userID)
        {
            var entity = await GetBaseQuery(UsersIncludes.None, true)
                .Where(o => o.GuildID == guildID.ToString() && o.UserID == userID.ToString())
                .Select(o => new { o.ID })
                .FirstOrDefaultAsync();

            return entity?.ID;
        }

        public async Task<UserEntity> GetOrCreateUserAsync(ulong guildID, ulong userID, UsersIncludes includes)
        {
            var entity = await GetUserAsync(guildID, userID, includes);

            if (entity == null)
            {
                entity = new UserEntity()
                {
                    GuildIDSnowflake = guildID,
                    UserIDSnowflake = userID
                };

                await Context.Users.AddAsync(entity);
            }

            return entity;
        }

        public Task<UserEntity> FindUserByApiTokenAsync(string apiToken, bool noTracking = false)
        {
            return GetBaseQuery(UsersIncludes.None, noTracking)
                .SingleOrDefaultAsync(o => o.ApiToken == apiToken);
        }

        public IQueryable<UserEntity> GetUsersWithBirthday(ulong guildID)
        {
            return GetBaseQuery(UsersIncludes.None, false)
                .Where(o => o.GuildID == guildID.ToString() && o.Birthday != null);
        }

        public async Task<int> CalculatePointsPositionAsync(ulong guildID, long userID)
        {
            var pointsList = await GetBaseQuery(UsersIncludes.None, true)
                .Where(o => o.GuildID == guildID.ToString() && o.Points > 0)
                .OrderByDescending(o => o.Points)
                .ThenBy(o => o.ID)
                .Select(o => o.ID)
                .ToListAsync();

            return pointsList.FindIndex(o => o == userID);
        }

        public IQueryable<UserEntity> GetUsersWithPointsOrder(ulong guildID, int skip, int take, bool asc)
        {
            var query = GetBaseQuery(UsersIncludes.None, false)
                .Where(o => o.GuildID == guildID.ToString() && o.Points > 0);

            if (asc)
                query = query.OrderBy(o => o.Points).ThenByDescending(o => o.ID);
            else
                query = query.OrderByDescending(o => o.Points).ThenBy(o => o.ID);

            return query
                .Skip(skip)
                .Take(take);
        }

        public IQueryable<UserEntity> GetUsersWithUnverify(ulong guildID)
        {
            return GetBaseQuery(UsersIncludes.Unverify, false)
                .Where(o => o.GuildID == guildID.ToString() && o.Unverify != null)
                .OrderBy(o => o.Unverify.EndDateTime);
        }

        public IQueryable<UserEntity> GetUsersWithUnverifyImunity(ulong guildID)
        {
            return GetBaseQuery(UsersIncludes.None, false)
                .Where(o => o.GuildID == guildID.ToString() && o.UnverifyImunityGroup != null);
        }

        public async Task<UserEntity> CreateAndGetUserAsync(ulong guildId, ulong userId)
        {
            await Context.Database.ExecuteSqlRawAsync(
                "INSERT INTO DiscordUsers (GuildID, UserID, Points, GivenReactionsCount, ObtainedReactionsCount, Flags) VALUES (@guild, @user, 0, 0, 0, 0)",
                new SqlParameter("@guild", guildId.ToString()),
                new SqlParameter("@user", userId.ToString())
            );

            return await GetUserAsync(guildId, userId, UsersIncludes.None);
        }

        public IQueryable<WebStatItem> GetWebStatisticsQuery()
        {
            return GetBaseQuery(UsersIncludes.None, false)
                .Where(o => o.WebAdminPassword != null || o.ApiToken != null)
                .Select(o => new WebStatItem()
                {
                    GuildId = o.GuildID,
                    UserId = o.UserID,
                    WebAdminLoginCount = o.WebAdminLoginCount,
                    HaveWebAdminAccess = o.WebAdminPassword != null,
                    Id = o.ID
                });
        }
    }
}
