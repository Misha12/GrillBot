using Grillbot.Database.Entity.Users;
using WebAdminUserOrder = Grillbot.Models.Users.WebAdminUserOrder;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace Grillbot.Database.Repository
{
    public class UsersRepository : RepositoryBase
    {
        public UsersRepository(GrillBotContext context) : base(context)
        {
        }

        private IQueryable<DiscordUser> GetBaseQuery(bool includeChannels)
        {
            var query = Context.Users.AsQueryable();

            if (includeChannels)
                query = query.Include(o => o.Channels);

            return query;
        }

        public IQueryable<DiscordUser> GetUsers(WebAdminUserOrder order, bool desc)
        {
            var query = GetBaseQuery(true);

            query = order switch
            {
                WebAdminUserOrder.Username => desc ? query.OrderByDescending(o => o.UserID) : query.OrderBy(o => o.UserID),
                WebAdminUserOrder.Server => desc ? query.OrderByDescending(o => o.GuildID) : query.OrderBy(o => o.GuildID),
                WebAdminUserOrder.Reactions => desc ? query.OrderByDescending(o => o.GivenReactionsCount).ThenByDescending(o => o.ObtainedReactionsCount)
                    : query.OrderBy(o => o.GivenReactionsCount).ThenBy(o => o.ObtainedReactionsCount),
                WebAdminUserOrder.Points => desc ? query.OrderByDescending(o => o.Points) : query.OrderBy(o => o.Points),
                WebAdminUserOrder.MessageCount => desc ? query.OrderByDescending(o => o.Channels.Sum(x => x.Count)) : query.OrderBy(o => o.Channels.Sum(x => x.Count)),
                _ => query.OrderByDescending(o => o.Channels.Sum(x => x.Count)),
            };
            return query;
        }

        public DiscordUser GetUser(ulong guildID, ulong userID, bool includeChannels = true)
        {
            var guild = guildID.ToString();
            var user = userID.ToString();

            var query = GetBaseQuery(includeChannels);
            return query.FirstOrDefault(o => o.GuildID == guild && o.UserID == user);
        }

        public DiscordUser GetUser(long id)
        {
            var query = GetBaseQuery(true);
            return query.FirstOrDefault(o => o.ID == id);
        }

        public DiscordUser GetOrCreateUser(ulong guildID, ulong userID, bool includeChannels = true)
        {
            var entity = GetUser(guildID, userID, includeChannels);

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
    }
}
