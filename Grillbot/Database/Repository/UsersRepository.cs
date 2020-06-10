using Grillbot.Database.Entity.Users;
using WebAdminUserOrder = Grillbot.Models.Users.WebAdminUserOrder;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Database.Repository
{
    public class UsersRepository : RepositoryBase
    {
        public UsersRepository(GrillBotContext context) : base(context)
        {
        }

        private IQueryable<DiscordUser> GetBaseQuery(bool includeChannels, bool includeBirthday, bool includeMathAudit)
        {
            var query = Context.Users.AsQueryable();

            if (includeChannels)
                query = query.Include(o => o.Channels);

            if (includeBirthday)
                query = query.Include(o => o.Birthday);

            if (includeMathAudit)
                query = query.Include(o => o.MathAudit);

            return query;
        }

        public IQueryable<DiscordUser> GetUsers(WebAdminUserOrder order, bool desc, ulong? guildID, int limit, ulong? userID)
        {
            var query = GetBaseQuery(true, false, false);

            if (guildID != null)
                query = query.Where(o => o.GuildID == guildID.ToString());

            if (userID != null)
                query = query.Where(o => o.UserID == userID.ToString());

            query = order switch
            {
                WebAdminUserOrder.UserID => desc ? query.OrderByDescending(o => o.ID) : query.OrderBy(o => o.ID),
                WebAdminUserOrder.Server => desc ? query.OrderByDescending(o => o.GuildID) : query.OrderBy(o => o.GuildID),
                WebAdminUserOrder.Reactions => desc ? query.OrderByDescending(o => o.GivenReactionsCount).ThenByDescending(o => o.ObtainedReactionsCount)
                    : query.OrderBy(o => o.GivenReactionsCount).ThenBy(o => o.ObtainedReactionsCount),
                WebAdminUserOrder.Points => desc ? query.OrderByDescending(o => o.Points) : query.OrderBy(o => o.Points),
                WebAdminUserOrder.MessageCount => desc ? query.OrderByDescending(o => o.Channels.Sum(x => x.Count)) : query.OrderBy(o => o.Channels.Sum(x => x.Count)),
                _ => query.OrderByDescending(o => o.Channels.Sum(x => x.Count)),
            };

            return query.Take(limit);
        }

        public DiscordUser GetUser(ulong guildID, ulong userID, bool includeChannels = true, bool includeBirthday = true, bool includeMathAudit = true)
        {
            var guild = guildID.ToString();
            var user = userID.ToString();

            var query = GetBaseQuery(includeChannels, includeBirthday, includeMathAudit);
            return query.FirstOrDefault(o => o.GuildID == guild && o.UserID == user);
        }

        public DiscordUser GetUserDetail(long id)
        {
            var query = GetBaseQuery(true, true, true);
            return query.FirstOrDefault(o => o.ID == id);
        }

        public async Task<long?> FindUserIDFromDiscordIDAsync(ulong guildID, ulong userID)
        {
            var guild = guildID.ToString();
            var user = userID.ToString();

            var query = GetBaseQuery(false, false, false);
            var entity = await query
                .SingleOrDefaultAsync(o => o.GuildID == guild && o.UserID == user);

            return entity?.ID;
        }

        public DiscordUser GetOrCreateUser(ulong guildID, ulong userID, bool includeChannels = true, bool includeBirthday = true, bool includeMathAudit = true)
        {
            var entity = GetUser(guildID, userID, includeChannels, includeBirthday, includeMathAudit);

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
            return await GetBaseQuery(false, false, false)
                .Select(o => o.UserID)
                .Distinct()
                .ToListAsync();
        }

        public DiscordUser FindUserByApiToken(string apiToken)
        {
            var query = GetBaseQuery(false, false, false);
            return query.FirstOrDefault(o => o.ApiToken == apiToken);
        }

        public List<DiscordUser> GetUsersWithBirthday(ulong guildID)
        {
            var guild = guildID.ToString();

            return GetBaseQuery(false, true, false)
                .Where(o => o.GuildID == guild && o.Birthday != null)
                .ToList();
        }
    }
}
