using Grillbot.Database.Entity.Users;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Database.Repository
{
    public class UserStatisticsRepository : RepositoryBase
    {
        public UserStatisticsRepository(GrillBotContext context) : base(context)
        {
        }

        private IQueryable<DiscordUser> GetBaseQuery()
        {
            return Context.Users
                .Include(o => o.Statistics)
                .AsQueryable();
        }

        public async Task IncrementApiCallCountAsync(string token)
        {
            var user = GetBaseQuery().SingleOrDefault(o => o.ApiToken == token);

            if (user == null)
                throw new InvalidOperationException($"Requested API ({token}) token was not found.");

            if (user.Statistics == null)
                user.Statistics = new StatisticItem();

            user.Statistics.ApiCallCount++;
            await Context.SaveChangesAsync();
        }

        public async Task IncrementWebAdminLoginCount(long userId)
        {
            var user = GetBaseQuery().SingleOrDefault(o => o.ID == userId);

            if (user == null)
                throw new InvalidOperationException($"User with ID ({userId}) not found.");

            if (user.Statistics == null)
                user.Statistics = new StatisticItem();

            user.Statistics.WebAdminLoginCount++;
            await Context.SaveChangesAsync();
        }
    }
}
