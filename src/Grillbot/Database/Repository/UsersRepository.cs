using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Grillbot.Enums;
using Grillbot.Database.Enums.Includes;
using Grillbot.Models.Users;
using UserEntity = Grillbot.Database.Entity.Users.DiscordUser;
using Grillbot.Database.Enums;

namespace Grillbot.Database.Repository
{
    public class UsersRepository : RepositoryBase
    {
        public UsersRepository(GrillBotContext context) : base(context)
        {
        }

        private IQueryable<UserEntity> GetBaseQuery(UsersIncludes includes)
        {
            var query = Context.Users.AsQueryable();

            if (includes.HasFlag(UsersIncludes.Channels))
                query = query.Include(o => o.Channels);

            if (includes.HasFlag(UsersIncludes.MathAudit))
                query = query.Include(o => o.MathAudit);

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

        public IQueryable<UserEntity> GetUsersQuery(UserListFilter filter, UsersIncludes includes)
        {
            var query = GetBaseQuery(includes)
                .Where(o => o.GuildID == filter.Guild.Id.ToString());

            if (filter.UserIDs.Count > 0)
            {
                var ids = filter.UserIDs.Select(o => o.ToString()).ToList();
                query = query.Where(o => ids.Contains(o.UserID));
            }

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

        public UserEntity GetUser(ulong guildID, ulong userID, UsersIncludes includes)
        {
            var guild = guildID.ToString();
            var user = userID.ToString();

            return GetBaseQuery(includes)
                .FirstOrDefault(o => o.GuildID == guild && o.UserID == user);
        }

        public Task<UserEntity> GetUserAsync(ulong guildID, ulong userID, UsersIncludes includes)
        {
            return GetBaseQuery(includes)
                .SingleOrDefaultAsync(o => o.GuildID == guildID.ToString() && o.UserID == userID.ToString());
        }

        public Task<UserEntity> GetUserAsync(long userID, UsersIncludes includes)
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

        public UserEntity GetOrCreateUser(ulong guildID, ulong userID, UsersIncludes includes)
        {
            var entity = GetUser(guildID, userID, includes);

            if (entity == null)
            {
                entity = new UserEntity()
                {
                    GuildIDSnowflake = guildID,
                    UserIDSnowflake = userID
                };

                Context.Users.Add(entity);
            }

            return entity;
        }

        public async Task<UserEntity> GetOrCreateUserAsync(ulong guildID, ulong userID, UsersIncludes includes)
        {
            var entity = await GetUserAsync(guildID, userID, includes);

            if(entity == null)
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

        public Task<UserEntity> FindUserByApiTokenAsync(string apiToken)
        {
            return GetBaseQuery(UsersIncludes.None)
                .SingleOrDefaultAsync(o => o.ApiToken == apiToken);
        }

        public IQueryable<UserEntity> GetUsersWithBirthday(ulong guildID)
        {
            return GetBaseQuery(UsersIncludes.None)
                .Where(o => o.GuildID == guildID.ToString() && o.Birthday != null);
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

        public IQueryable<UserEntity> GetUsersWithPointsOrder(ulong guildID, int skip, int take, bool asc)
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

        public IQueryable<UserEntity> GetUsersWithUsedCode(ulong guildID, string code)
        {
            return Context.Users.AsQueryable()
                .Where(o => o.GuildID == guildID.ToString() && o.UsedInviteCode == code);
        }

        public IQueryable<UserEntity> GetUsersWithUnverify(ulong guildID)
        {
            return GetBaseQuery(UsersIncludes.Unverify)
                .Where(o => o.GuildID == guildID.ToString() && o.Unverify != null);
        }
    }
}
