using Discord.WebSocket;
using Grillbot.Database.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Database.Repository
{
    public class BirthdaysRepository : RepositoryBase
    {
        public BirthdaysRepository(GrillBotContext context) : base(context)
        {
        }

        public async Task<Birthday> AddBirthdayAsync(bool acceptAge, DateTime date, ulong guildID, ulong authorID)
        {
            var entity = new Birthday()
            {
                Date = date.Date,
                GuildIDSnowflake = guildID,
                IDSnowflake = authorID,
                AcceptAge = acceptAge
            };

            await Context.Set<Birthday>().AddAsync(entity).ConfigureAwait(false);
            await Context.SaveChangesAsync().ConfigureAwait(false);

            return entity;
        }

        public async Task<List<Birthday>> GetBirthdaysForDayAsync(DateTime date, string guildID)
        {
            var result = new List<Birthday>();

            var query = Queryable.Where(Context.Birthdays, o => o.GuildID == guildID);
            foreach (var birthday in query.ToList())
            {
                if (birthday.Date.Day == date.Day && birthday.Date.Month == date.Month)
                {
                    result.Add(birthday);
                }
            }

            return result;
        }

        public async Task<bool> ExistsUserAsync(SocketUser user, string guildID)
        {
            var userID = user.Id.ToString();
            return Context.Birthdays.Any(o => o.GuildID == guildID && o.ID == userID);
        }

        public async Task RemoveAsync(SocketUser user, string guildID)
        {
            var userID = user.Id.ToString();
            var entity = Context.Birthdays.FirstOrDefault(o => o.GuildID == guildID && o.ID == userID);

            if (entity == null) return;

            Context.Set<Birthday>().Remove(entity);
            await Context.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
